//---------------------------------------------------------------------
// <copyright file="DogsController.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved.
// </copyright>
//---------------------------------------------------------------------

namespace Microsoft.Workload.Errors.WebAPIODataCoreSample.Controllers
{
    using System.Linq;
    using Microsoft.AspNet.OData;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Workload.Errors.WebAPIODataCoreSample.ErrorHandling;
    using Microsoft.Workload.Errors.WebAPIODataCoreSample.Models;

    public class DogsController : ODataController
    {
        private readonly DogContext context;

        public DogsController(DogContext context)
        {
            this.context = context;

            if (!context.Dogs.Any())
            {
                context.Dogs.AddRange(SampleData.GetDogs());
                context.SaveChanges();
            }
        }

        [EnableQuery]
        public IActionResult Get()
        {
            return Ok(context.Dogs);
        }

        [EnableQuery]
        public IActionResult Get(int key)
        {
            // Contrived example of not allowing a specific Dog to be accessed
            if (key == 5)
            {
                // Assume that there was auth-related code that resolves to an exception.
                throw new AccessDeniedException(
                    Utilities.Constants.ErrorCodeOwnershipRestriction,
                    string.Format("Access to Dog({0}) is restricted to only its owner.", key));
            }

            return Ok(context.Dogs.FirstOrDefault(dog => dog.Id == key));
        }

        [HttpPost]
        public IActionResult Post(Dog dog)
        {
            context.Dogs.Add(dog);
            context.SaveChanges();
            return Created(dog);
        }
    }
}