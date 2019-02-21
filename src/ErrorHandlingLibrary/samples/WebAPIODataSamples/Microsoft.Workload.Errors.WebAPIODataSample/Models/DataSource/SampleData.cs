//---------------------------------------------------------------------
// <copyright file="SampleData.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved.
// </copyright>
//---------------------------------------------------------------------

namespace Microsoft.Workload.Errors.WebAPIODataSample.Models
{
    using System.Collections.Generic;

    internal static class SampleData
    {
        private static IList<Dog> dogs = null;

        public static IList<Dog> GetDogs()
        {
            if (dogs != null)
            {
                return dogs;
            }

            dogs = new List<Dog>
            {
                new Dog
                {
                    Id = 1,
                    Name = "ShibSibs"
                },
                new Dog
                {
                    Id = 2,
                    Name = "YungPupper"
                },
                new Dog
                {
                    Id = 3,
                    Name = "LazyDoggo"
                },
                new Dog
                {
                    Id = 4,
                    Name = "Satya"
                },
                new Dog
                {
                    Id = 5,
                    Name = "AccessDenied"
                }
            };

            return dogs;
        }
    }
}