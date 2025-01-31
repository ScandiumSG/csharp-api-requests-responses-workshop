﻿using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using workshop.wwwapi.Models.Cars;
using workshop.wwwapi.Repository;

namespace workshop.wwwapi.Endpoints
{
    public static class CarEndpoint
    {
        public static void ConfigureCarEndpoint(this WebApplication app)
        {
            var carGroup = app.MapGroup("cars");

            carGroup.MapGet("/", GetCars);
            carGroup.MapPost("/", AddCar).AddEndpointFilter(async (invocationContext, next) =>
            {
                var car = invocationContext.GetArgument<CarPost>(1);

                if (string.IsNullOrEmpty(car.Make) || string.IsNullOrEmpty(car.Model))
                {
                    return Results.BadRequest("You must enter a Make AND Model");
                }
                return await next(invocationContext);
            }); ;
            carGroup.MapPut("/{id}", UpdateCar);
            carGroup.MapGet("/{id}", GetACars);
            carGroup.MapDelete("/{id}", DeleteCar);
        }
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public static async Task<IResult> DeleteCar(IRepository<Car> repository, int id)
        {
            if (!repository.Get().Any(x => x.Id == id))
            {
                return TypedResults.NotFound("Car not found.");
            }
            var result = repository.Delete(id);
            return result != null ? TypedResults.Ok(result) : Results.NotFound();
        }
        [ProducesResponseType(StatusCodes.Status200OK)]
        public static async Task<IResult> GetCars(IRepository<Car> repository)
        {
            return TypedResults.Ok(repository.Get());
        }
        [ProducesResponseType(StatusCodes.Status200OK)]
        public static async Task<IResult> GetACars(IRepository<Car> repository, int id)
        {
            return TypedResults.Ok(repository.GetById(id));
        }

        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public static async Task<IResult> AddCar(IRepository<Car> repository, CarPost model)
        {
            if (repository.Get().Any(x => x.Model.Equals(model.Model, StringComparison.OrdinalIgnoreCase)))
            {
                return Results.BadRequest("Product with provided name already exists");
            }
            
            var entity = new Car() { Make=model.Make, Model=model.Model};
            repository.Insert(entity);
            return TypedResults.Created($"/{entity.Id}", entity);
         
        }

        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status201Created)]

        public static async Task<IResult> UpdateCar(IRepository<Car> repository, int id, CarPut model)
        {
            if (!repository.Get().Any(x => x.Id == id))
            {
                return TypedResults.NotFound("Product not found.");
            }
            var entity = repository.GetById(id);

            if (model.Model != null)
            {
                if (repository.Get().Any(x => x.Model == model.Model))
                {
                    return Results.BadRequest("Product with provided name already exists");
                }
            }
            entity.Make = model.Make != null ? model.Make : entity.Make;
            entity.Model = model.Model != null ? model.Model : entity.Model;
            
            repository.Update(entity);

            return TypedResults.Created($"/{entity.Id}", entity);

        }

    }
}
