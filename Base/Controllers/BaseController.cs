using AutoMapper;
using CharCode.Base.Abstraction;
using CharCode.Base.Classes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CharCode.Base.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize]
    public abstract class BaseController<TRepository, TModel, TViewModel, TKey> : ControllerBase
        where TModel : class, IModel<TKey>
        where TViewModel : class, IViewModel<TKey>
        where TRepository : IBaseRepository<TModel, TKey>
    {
        protected readonly TRepository repository;
        protected readonly IMapper mapper;

        public BaseController(TRepository repository, IMapper mapper)
        {
            this.repository = repository;
            this.mapper = mapper;
        }

        [HttpPost("{id}")]
        public virtual async Task<IActionResult> GetAsync(TKey id)
        {
            var item = await repository.GetAsync(id);

            var result = mapper.Map<TViewModel>(item);

            return Ok(result);
        }

        protected virtual PaginationConfig GetConfig(PaginationConfig config)
        {
            foreach (var filter in config.ExpressionFilters)
                filter.PropertyName = GetColumnName(filter.PropertyName);

            config.SortColumn = GetColumnName(config.SortColumn);

            return config;
        }


        protected virtual string GetColumnName(string name)
        {
            switch (name.ToLower())
            {
                default:
                    return name;
            }
        }

        [HttpPost]
        public virtual async Task<IActionResult> GetAsync([FromBody]PaginationConfig config)
        {
            config = GetConfig(config);

            var itemsList = await repository.GetAsync(config);
            var count = await repository.GetCountAsync(config);

            var items = itemsList.Select(i => mapper.Map<TViewModel>(i)).ToList();

            var responece = new TableResponce<TViewModel>()
            {
                Items = items,
                TotalCount = count
            };

            return Ok(responece);
        }

        [HttpPost("{id}")]
        public virtual async Task<IActionResult> UpdateAsync(TKey id, [FromBody]TViewModel obj)
        {
            if (!ModelState.IsValid || !id.Equals(obj.Id))
                return BadRequest();

            var modelObject = mapper.Map<TModel>(obj);

            await repository.UpdateAsync(id, modelObject);

            return Ok();
        }

        [HttpPost]
        public virtual async Task<IActionResult> InsertAsync([FromBody]TViewModel obj)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var modelObject = mapper.Map<TModel>(obj);

            var insertedObject = await repository.InsertAsync(modelObject);

            var result = mapper.Map<TViewModel>(insertedObject);

            return Ok(result);
        }

        [HttpPost("{id}")]
        public virtual async Task<IActionResult> DeleteAsync(TKey id)
        {
            await repository.DeleteAsync(id);

            return Ok();
        }
    }

    public abstract class BaseController<TManager, TModel, TViewModel> : BaseController<TManager, TModel, TViewModel, long>
        where TModel : class, IModel
        where TViewModel : class, IViewModel
        where TManager : IBaseRepository<TModel>
    {
        public BaseController(TManager manager, IMapper mapper) : base(manager, mapper)
        {
        }
    }
}
