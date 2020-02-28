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
    public abstract class BringoBaseController<TManager, TModel, TViewModel, TKey> : ControllerBase
        where TModel : class, IModel<TKey>
        where TViewModel : class, IViewModel<TKey>
        where TManager : IBaseRepository<TModel, TKey>
    {
        protected readonly TManager _manager;
        protected readonly IMapper _mapper;

        public BringoBaseController(TManager manager, IMapper mapper)
        {
            _manager = manager;
            _mapper = mapper;
        }

        [HttpPost("{id}")]
        public async Task<ActionResult<TViewModel>> GetAsync(TKey id)
        {
            var item = await _manager.GetAsync(id);

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

            var itemsList = await _manager.GetAsync(config);
            var count = await _manager.GetCountAsync(config);

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

            await _manager.UpdateAsync(id, modelObject);

            return Ok();
        }

        [HttpPost]
        public virtual async Task<IActionResult> InsertAsync([FromBody]TViewModel obj)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var modelObject = _mapper.Map<TModel>(obj);

            var insertedObject = await _manager.InsertAsync(modelObject);

            var result = _mapper.Map<TViewModel>(insertedObject);

            return Ok(result);
        }
    }

    public abstract class BringoBaseController<TManager, TModel, TViewModel> : BringoBaseController<TManager, TModel, TViewModel, long>
        where TModel : class, IModel
        where TViewModel : class, IViewModel
        where TManager : BaseRepository<TModel>
    {
        public BringoBaseController(TManager manager, IMapper mapper) : base(manager, mapper)
        {
        }
    }
}
