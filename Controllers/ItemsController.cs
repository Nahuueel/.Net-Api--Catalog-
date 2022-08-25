using Catalog.Dtos;
using Catalog.Entities;
using Catalog.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Catalog.Controllers{

    //GET /items
    [ApiController]
    [Route("items")]
    public class ItemsController : ControllerBase{

        private readonly IItemsRepository repository; 

        public ItemsController(IItemsRepository repository){
            this.repository = repository; 
        }

        // GET /items
        [HttpGet]
        public IEnumerable<ItemDto> GetItems(){
            
            var items = repository.GetItems().Select( item => item .AsDto()); 
            return items;
        }

        // GET /items/{id}
        [HttpGet("{id}")]
        public ActionResult<ItemDto> GetItem(Guid id){

            var item = repository.GetItem(id);

            if(item is null){
                
                return NotFound();
            }

            return  Ok(item.AsDto());
        }

        [HttpPost]
        public ActionResult<ItemDto> CreateItem(CreateItemDto itemDto){

            Item item = new(){
                Id = Guid.NewGuid(),
                Name = itemDto.Name,
                Price = itemDto.Price,
                CreatedDate = DateTimeOffset.UtcNow
            };

            repository.CreateItem(item);

            return CreatedAtAction(nameof(GetItem), new { Id = item.Id}, item.AsDto()); 

        }

        // PUT /items/id
        [HttpPut("{id}")]
        public ActionResult UpdateItem(Guid id, UpdateItemDto itemDto){

            var existingItem = repository.GetItem(id);

            if (existingItem is null){

                return NotFound(); 
            }

            Item updatedItem = existingItem with {
                Name = itemDto.Name,
                Price = itemDto.Price
            }; 

            repository.UpdateItem(updatedItem); 

            return Ok(); 
        }
        
        // DELETE items/id/

        [HttpDelete("{id}")]
        public ActionResult DeleteItem(Guid id){

            var existingItem = repository.GetItem(id);

            if (existingItem is null){

                return NotFound(); 
            }

            repository.DeleteItem(id);
            
            return Ok();
        }

    }
}