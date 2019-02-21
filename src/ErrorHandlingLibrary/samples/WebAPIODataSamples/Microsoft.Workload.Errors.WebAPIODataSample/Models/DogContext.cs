//---------------------------------------------------------------------
// <copyright file="DogContext.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved.
// </copyright>
//---------------------------------------------------------------------

namespace Microsoft.Workload.Errors.WebAPIODataSample.Models
{
    using System.Data.Entity;

    internal class DogContext : DbContext
    {
        public DbSet<Dog> Dogs { get; set; }
    }
}