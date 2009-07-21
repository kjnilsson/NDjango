﻿(****************************************************************************
 * 
 *  NDjango Parser Copyright © 2009 Hill30 Inc
 *
 *  This file is part of the NDjango Parser.
 *
 *  The NDjango Parser is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU Lesser General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  The NDjango Parser is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU Lesser General Public License for more details.
 *
 *  You should have received a copy of the GNU Lesser General Public License
 *  along with NDjango Parser.  If not, see <http://www.gnu.org/licenses/>.
 *  
 ***************************************************************************)

#light

namespace NDjango

open NDjango.Lexer
open NDjango.Interfaces
open NDjango.OutputHandling
open NDjango.Expressions

module Compatibility =

    /// Abstract tag implementation designed for consumption outside of F#. This class
    /// will handle interaction with the expression system and the parser, providing
    /// the abstract 'ProcessTag' method with the execution-time values of the supplied
    /// parameters, along with the fully resolved text of the nested value (if in nested mode).
    /// Concrete implementations are required to define a 0-parameter constructor, and
    /// pass in relevant values for 'nested' and 'name'. The value of 'name' must match
    /// the name the tag is registered under, but is only applicable when 'nested' is true.
    [<AbstractClass>]
    type public SimpleTag(nested:bool, name:string, num_params:int) =
        /// Resolves all expressions in the list against the context
        let resolve_all list (context: IContext) =
            list |>
            List.map (fun (e: FilterExpression) -> 
                        match fst <| e.Resolve context true with
                        | None -> null
                        | Some v -> v) |>
            List.to_array
    
        /// Evaluates the contents of the nodelist against the given walker. This function
        /// effectively parses the nested tags within the simple tag.
        let read_walker walker nodelist =
            let reader = 
                new NDjango.ASTWalker.Reader ({walker with parent=None; nodes=nodelist; context=walker.context})
            reader.ReadToEnd()
            
        /// Tag implementation. This method will receive the fully-evaluated nested contents for nested tag
        /// along with fully resolved values of the parameters supplied to the tag. Parameters in the template
        /// source may follow standard parameter conventions, e.g. they can be variables or literals, with 
        /// filters.
        abstract member ProcessTag: string -> obj array -> string 
        
        interface ITag with
            member x.Perform token parser tokens = 
                let parms = 
                    token.Args |>
                    List.map (fun elem -> new FilterExpression(parser, Block token, elem))
                
                if not (parms.Length = num_params) then
                    raise (TemplateSyntaxError(sprintf "%s expects %d parameters, but was given %d." name num_params (parms.Length), Some (token:>obj)))
                else
                    let nodelist, tokens =
                        if nested then parser.Parse tokens ["end" + name]
                        else [], tokens
                        
                    {
                        new Node(Block token)
                        with
                            override this.walk walker =
                                let resolved_parms =  resolve_all parms walker.context
                                if not nested then
                                    {walker with parent = Some walker; buffer = (x.ProcessTag "" resolved_parms)}
                                else
                                    {walker with parent = Some walker; buffer = (x.ProcessTag (read_walker walker nodelist) resolved_parms)}
                    }, tokens