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
        protected readonly TRepository _repository;
        protected readonly IMapper _mapper;

        public BaseController(TRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        [HttpPost("{id}")]
        public async Task<ActionResult<TViewModel>> GetAsync(TKey id)
        {
            var item = await _repository.GetAsync(id);

            var result = _mapper.Map<TViewModel>(item);

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
        public virtual async Task<ActionResult<TableResponce<TViewModel>>> GetAsync([FromBody]PaginationConfig config)
        {
            config = GetConfig(config);

            var itemsList = await _repository.GetAsync(config);
            var count = await _repository.GetCountAsync(config);

            var items = itemsList.Select(i => _mapper.Map<TViewModel>(i)).ToList();

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

            var modelObject = _mapper.Map<TModel>(obj);

            await _repository.UpdateAsync(id, modelObject);

            return Ok();
        }

        [HttpPost]
        public virtual async Task<ActionResult<TViewModel>> InsertAsync([FromBody]TViewModel obj)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var modelObject = _mapper.Map<TModel>(obj);

            var insertedObject = await _repository.InsertAsync(modelObject);

            var result = _mapper.Map<TViewModel>(insertedObject);

            return Ok(result);
        }

        [HttpPost("{id}")]
        public virtual async Task<IActionResult> DeleteAsync(TKey id)
        {
            await _repository.DeleteAsync(id);

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
