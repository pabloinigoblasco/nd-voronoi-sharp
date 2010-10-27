﻿/*  Copyright (C) 2010  Pablo Iñigo Blasco. 
    Computer Architecture and Technology Department.
    Universidad de Sevilla. 

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.*/

using System;
using System.Collections.Generic;
using System.Linq;

internal static class Helpers
{
    internal static IEnumerable<IEnumerable<T>> Combinations<T>(IEnumerable<T> elements, int setLenght)
    {
        int elementLenght = elements.Count();
        if (setLenght == 1)
            return elements.Select(e => Enumerable.Repeat(e, 1));
        else if (setLenght == elementLenght)
            return Enumerable.Repeat(elements, 1);
        else
        {
            return Combinations(elements.Skip(1), setLenght - 1)
                            .Select(tail => Enumerable.Repeat(elements.First(), 1).Union(tail))
                            .Union(Combinations(elements.Skip(1), setLenght));
        }
    }
}