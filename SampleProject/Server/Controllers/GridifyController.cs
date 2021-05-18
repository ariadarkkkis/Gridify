using AutoMapper;
using AutoMapper.QueryableExtensions;
using Gridify;
using Gridify.EntityFramework;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SampleProject.Entites;
using SampleProject.DataTransferObjects;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SampleProject.Controllers
{
   [ApiController]
   [Route("/api/[controller]")]
   public class GridifyController : ControllerBase
   {
      private readonly AppDbContext _context;
      private readonly IMapper _mapper;

      public GridifyController(AppDbContext context, IMapper mapper)
      {
         this._context = context;
         this._mapper = mapper;
      }

      /// <summary>
      /// Returns Gridify Paging List of PersonDto
      /// </summary>
      /// <param name="gridifyQuery"></param>
      /// <returns></returns>
      [HttpGet]
      [Produces(typeof(Paging<PersonDto>))]
      public async Task<IActionResult> Get([FromQuery] GridifyQuery gridifyQuery)
      {
         // Simple usage of gridify with AutoMapper
         return Ok(await _context.People.AsNoTracking()
            .ProjectTo<PersonDto>(_mapper.ConfigurationProvider)
            .GridifyAsync(gridifyQuery));
      }

      /// <summary>
      /// Returns a List of PersonDto with only Filtering applied (Simple filter)
      /// </summary>
      /// <param name="gridifyQuery"></param>
      /// <returns></returns>
      [HttpGet("SimpleFilter")]
      [Produces(typeof(List<PersonDto>))]
      public async Task<IActionResult> GetSimpleFilteredList([FromQuery] GridifyQuery gridifyQuery)
      {
         if (string.IsNullOrEmpty(gridifyQuery.Filter))
         {
            // Simple filter
            // Usually we don't need create this object manually, We get it from Query
            gridifyQuery = new GridifyQuery()
            {
               // FirstName equals to Alireza
               Filter = "FirstName==Alireza"
            };
         }
         var result = _context.People.ApplyFiltering(gridifyQuery);
         return Ok(await result.ProjectTo<PersonDto>(_mapper.ConfigurationProvider).ToListAsync());
      }

      /// <summary>
      /// Returns a List of PersonDto with only Filtering applied (Complex filter)
      /// </summary>
      /// <param name="gridifyQuery"></param>
      /// <returns></returns>
      [HttpGet("ComplexFilter")]
      [Produces(typeof(List<PersonDto>))]
      public async Task<IActionResult> GetComplexFilteredList([FromQuery] GridifyQuery gridifyQuery)
      {
         if (string.IsNullOrEmpty(gridifyQuery.Filter))
         {
            // Complex filter
            gridifyQuery = new GridifyQuery()
            {
               // FirstName contains Ali AND FirstName doesn't contain reza
               // OR
               // FirstName contains Ali AND Age is greater than 30
               Filter = "(FirstName=*Ali,FirstName!*reza)|(FirstName=*Ali,Age>>30)"
            };
         }
         var result = _context.People.ApplyFiltering(gridifyQuery);
         return Ok(await result.ProjectTo<PersonDto>(_mapper.ConfigurationProvider).ToListAsync());
      }

      /// <summary>
      /// Returns a List of PersonDto with only Ordering applied
      /// </summary>
      /// <param name="gridifyQuery"></param>
      /// <returns></returns>
      [HttpGet("Ordering")]
      [Produces(typeof(List<PersonDto>))]
      public async Task<IActionResult> GetOrderedList([FromQuery] GridifyQuery gridifyQuery)
      {
         if (string.IsNullOrEmpty(gridifyQuery.SortBy))
         {
            gridifyQuery = new GridifyQuery()
            {
               // Decending order
               IsSortAsc = false,
               SortBy = "Age"
            };
         }
         var result = _context.People.ApplyOrdering(gridifyQuery);
         return Ok(await result.ProjectTo<PersonDto>(_mapper.ConfigurationProvider).ToListAsync());
      }

      /// <summary>
      /// Returns Gridify Paging List of PersonDto with only Paging applied
      /// </summary>
      /// <param name="gridifyQuery"></param>
      /// <returns></returns>
      [HttpGet("Paging")]
      [Produces(typeof(Paging<PersonDto>))]
      public async Task<IActionResult> GetPaginatedList([FromQuery] GridifyQuery gridifyQuery)
      {
         if (gridifyQuery.PageSize <= 0 && gridifyQuery.Page <= 0)
         {
            gridifyQuery = new GridifyQuery()
            {
               // Page is defined by client, or send in URL query.
               Page = 1,
               PageSize = 20
            };
         }
         var result = _context.People.ApplyPaging(gridifyQuery);
         return Ok(await result.ProjectTo<PersonDto>(_mapper.ConfigurationProvider).ToListAsync());
      }

      /// <summary>
      /// Returns Gridify Paging List of PersonDto with Ordering and Paging applied 
      /// </summary>
      /// <param name="gridifyQuery"></param>
      /// <returns></returns>
      [HttpGet("OrderingAndPaging")]
      [Produces(typeof(Paging<PersonDto>))]
      public async Task<IActionResult> GetOrderedAndPaginatedList([FromQuery] GridifyQuery gridifyQuery)
      {
         if ((gridifyQuery.PageSize <= 0 && gridifyQuery.Page <= 0) || string.IsNullOrEmpty(gridifyQuery.SortBy))
         {
            gridifyQuery = new GridifyQuery()
            {
               // Ascending order
               IsSortAsc = true,
               SortBy = "Age",
               PageSize = 2
               // Page is defined by client, or send in URL query.
            };
         }
         var result = _context.People.ApplyOrderingAndPaging(gridifyQuery);
         return Ok(await result.ProjectTo<PersonDto>(_mapper.ConfigurationProvider).ToListAsync());
      }

      /// <summary>
      /// Returns Gridify Paging List of PersonDto with Everything applied
      /// </summary>
      /// <param name="gridifyQuery"></param>
      /// <returns></returns>
      [HttpGet("Everything")]
      [Produces(typeof(Paging<PersonDto>))]
      public async Task<IActionResult> GetEverythingList([FromQuery] GridifyQuery gridifyQuery)
      {
         if (string.IsNullOrEmpty(gridifyQuery.Filter) ||
            gridifyQuery.PageSize <= 0 || gridifyQuery.Page <= 0)
         {
            gridifyQuery = new GridifyQuery()
            {
               // Exclude a specific LastName from result
               Filter = "LastName!*D",
               // Ascending order
               IsSortAsc = true,
               SortBy = "Address",
               PageSize = 10,
               // Page is defined by client, or send in URL query.
               Page = 1
            };
         }
         var result = _context.People.ApplyEverything(gridifyQuery);
         return Ok(await result.ProjectTo<PersonDto>(_mapper.ConfigurationProvider).ToListAsync());
      }

      /// <summary>
      /// Returns Gridify Paging List of PersonDto with CustomMapping
      /// </summary>
      /// <param name="gridifyQuery"></param>
      /// <returns></returns>
      [HttpGet("CustomMapping")]
      [Produces(typeof(Paging<PersonDto>))]
      public async Task<IActionResult> GetCustomMappedList([FromQuery] GridifyQuery gridifyQuery)
      {
         // Case sensetive is false by default. But we can enable it here.
         var customMappings = new GridifyMapper<Person>(false)
           // Because properties with same name exists in both DTO and Entity classes, we can Generate them.
           .GenerateMappings()
           // Add custom mappings
           .AddMap("livingAddress", q => q.Contact.Address)
           .AddMap("phone", q => q.Contact.PhoneNumber.ToString());

         // GridifyQueryable return a QueryablePaging<T>
         var result = await _context.People.GridifyQueryableAsync(gridifyQuery, customMappings);

         // We then apply AutoMapper to the query result and return a Paging.
         return Ok(new Paging<PersonDto>()
         {
            Items = result.Query.ProjectTo<PersonDto>(_mapper.ConfigurationProvider).ToList(),
            TotalItems = result.TotalItems
         });
      }
   }
}
