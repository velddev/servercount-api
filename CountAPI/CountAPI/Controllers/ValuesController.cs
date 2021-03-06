﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CountAPI.Authentication;
using CountAPI.Common;
using CountAPI.Results;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace CountAPI.Controllers
{
	[Route("api/[controller]")]
	public class CountController : Controller
	{
		public static List<Shard> allShards = new List<Shard>();

		public static event Func<int, Task> OnGuildChange;

		private JsonSerializerSettings serializerSettings = new JsonSerializerSettings()
		{
			ContractResolver = new DefaultContractResolver()
			{
				NamingStrategy = new SnakeCaseNamingStrategy()
			}
		};

		// GET api/count/
		[HttpGet()]
		public string Get()
		{
			return JsonConvert.SerializeObject(new
			{
				ShardCount = allShards.Count,
				ServerCount = allShards.Sum(x => x.Count)
			}, serializerSettings);
		}
		// GET api/count/shards
		[HttpGet("{shards}")]
		public string GetShards()
		{
			return JsonConvert.SerializeObject(allShards, serializerSettings);
		}
		// GET api/values/5
		[HttpGet("{id}")]
		public string Get(int id)
			=> JsonConvert.SerializeObject(allShards[id], serializerSettings);

        // POST api/values
        [HttpPost]
		[BasicAuthentication]
        public async Task<JsonResult> Post([FromBody]Shard s)
        {
			if(s != null)
			{ 
				if (allShards.Count > s.Id)
				{
					allShards[s.Id] = new Shard(s.Id, s.Count);
				}
				else
				{
					while (allShards.Count != s.Id)
					{
						allShards.Add(new Shard(0, 0));
					}

					allShards.Add(new Shard(s.Id, s.Count));
				}
			}
			else
			{
				return new ErrorResult(500, "Body not supplied");
			}

			await OnGuildChange(allShards.Sum(x => x.Count));
			return new ErrorResult(200, "OK");
		}
    }
}
