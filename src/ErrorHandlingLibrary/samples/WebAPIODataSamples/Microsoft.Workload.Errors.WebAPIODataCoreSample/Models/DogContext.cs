//---------------------------------------------------------------------
// <copyright file="DogContext.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved.
// </copyright>
//---------------------------------------------------------------------

namespace Microsoft.Workload.Errors.WebAPIODataCoreSample.Models
{
    using Microsoft.EntityFrameworkCore;

    public class DogContext : DbContext
    {
        public DogContext(DbContextOptions<DogContext> options)
            : base(options)
        {
        }

        public DbSet<Dog> Dogs { get; set; }
    }
}