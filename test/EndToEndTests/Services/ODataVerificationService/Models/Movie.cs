//---------------------------------------------------------------------
// <copyright file="Movie.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

using System;

namespace ODataVerificationService.Models
{
    public class Movie
    {
        public int ID { get; set; }

        public string Title { get; set; }

        public int Year { get; set; }

        public DateTimeOffset? DueDate { get; set; }

        public bool IsCheckedOut
        {
            get { return DueDate.HasValue; }
        }
    }
}