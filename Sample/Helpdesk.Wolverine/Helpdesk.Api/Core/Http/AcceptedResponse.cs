// using System.Reflection;
// using JasperFx.CodeGeneration.Frames;
// using JasperFx.CodeGeneration.Model;
// using Microsoft.AspNetCore.Http.Metadata;
// using Microsoft.Extensions.Primitives;
// using Wolverine.Http;
//
// namespace Helpdesk.Api.Core.Http;
//
// public record AcceptedResponse(string Url) : IHttpAware, IEndpointMetadataProvider
// {
//     public static void PopulateMetadata(MethodInfo method, EndpointBuilder builder)
//     {
//         builder.RemoveStatusCodeResponse(200);
//
//         var create = new MethodCall(method.DeclaringType!, method).Creates.FirstOrDefault()?.VariableType;
//         var metadata = new WolverineProducesResponseTypeMetadata { Type = create, StatusCode = 201 };
//         builder.Metadata.Add(metadata);
//     }
//
//     void IHttpAware.Apply(HttpContext context)
//     {
//         context.Response.Headers.Location = Url;
//         context.Response.StatusCode = 201;
//     }
//
//     public static CreationResponse<T> For<T>(T value, string url) => new CreationResponse<T>(url, value);
// }
