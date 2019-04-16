﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using System;
using System.IO;
using System.Linq;
using FreeSql.Extensions.EntityUtil;
using System.Threading.Tasks;
using System.Text;
using System.Collections.Generic;
using System.Reflection;

namespace FreeSql {
	public static class FreeSqlAdminLTEExtesions {

		public static IApplicationBuilder UseFreeAdminLTE(this IApplicationBuilder app, string requestPathBase, params Type[] entityTypes) {

			requestPathBase = requestPathBase.ToLower();
			if (requestPathBase.StartsWith("/") == false) requestPathBase = $"/{requestPathBase}";
			if (requestPathBase.EndsWith("/") == false) requestPathBase = $"{requestPathBase}/";
			var restfulRequestPath = $"{requestPathBase}restful-api";

			IFreeSql fsql = app.ApplicationServices.GetService(typeof(IFreeSql)) as IFreeSql;
			if (fsql == null) throw new Exception($"UseFreeAdminLTE 错误，找不到 IFreeSql，请提前注入");

			var dicEntityTypes = entityTypes.ToDictionary(a => a.Name);

			app.UseFreeAdminLTEStaticFiles(requestPathBase);

			app.Use(async (context, next) => {

				var req = context.Request;
				var res = context.Response;
				var reqPath = req.Path.Value.ToLower();
				if (reqPath.EndsWith("/") == false) reqPath = $"{reqPath}/";

				try {
					if (reqPath == requestPathBase) {
						//首页
						var sb = new StringBuilder();
						sb.AppendLine(@"<ul class=""treeview-menu"">");
						foreach (var et in dicEntityTypes) {
							sb.AppendLine($@"<li><a href=""{requestPathBase}{et.Key}/""><i class=""fa fa-circle-o""></i>{et.Key}</a></li>");
						}
						sb.AppendLine(@"</ul>");
						await res.WriteAsync(Views.Index.Replace(@"<ul class=""treeview-menu""></ul>", sb.ToString()));
						return;
					}
					else if (reqPath.StartsWith(restfulRequestPath)) {
						//动态接口
						if (await Restful.Use(context, fsql, restfulRequestPath, dicEntityTypes)) return;
					}
					else if (reqPath.StartsWith(requestPathBase)) {
						//前端UI
						if (await Admin.Use(context, fsql, requestPathBase, dicEntityTypes)) return;
					}

				} catch (Exception ex) {
					await Utils.Jsonp(context, new { code = 500, message = ex.Message });
					return;
				}
				await next();
			});

			return app;
		}
	}
}
