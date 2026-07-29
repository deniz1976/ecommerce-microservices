using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Errors;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.BuildingBlocks.Security;
using ECommerce.Catalog.Application;
using ECommerce.Catalog.Application.Commands.ManageProductImage;
using ECommerce.Catalog.Application.Images;
using ECommerce.Catalog.Application.Products;

namespace ECommerce.Catalog.Api.Products;

public static class ProductImageEndpoints
{
    public static IEndpointRouteBuilder MapProductImageEndpoints(this IEndpointRouteBuilder endpoints)
    {
        RouteGroupBuilder group = endpoints
            .MapGroup("/api/v1/products/{productId:guid}/images")
            .WithTags("Product Images")
            .RequireAuthorization(AuthorizationPolicies.SellerOrAdmin);

        group.MapPost("/", UploadAsync)
            .WithName("UploadProductImage")
            .Accepts<IFormFile>("multipart/form-data")
            .DisableAntiforgery();

        group.MapPut("/{imageId:guid}/main", SetMainAsync)
            .WithName("SetMainProductImage");

        group.MapDelete("/{imageId:guid}", DeleteAsync)
            .WithName("DeleteProductImage");

        return endpoints;
    }

    private static async Task<IResult> UploadAsync(
        Guid productId,
        HttpRequest request,
        ICommandHandler<
            ManageProductImageCommand,
            Result<ProductImageResponse>> commandHandler,
        IAuthenticatedUserResolver userResolver,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        if (!request.HasFormContentType ||
            request.ContentLength > ProductImageUploadValidator.MaxFileSize + (64 * 1024))
        {
            return InvalidImage(httpContext);
        }

        IFormCollection form;
        try
        {
            form = await request.ReadFormAsync(cancellationToken);
        }
        catch (InvalidDataException)
        {
            return InvalidImage(httpContext);
        }

        IFormFile? file = form.Files.GetFile("file");
        if (file is null)
        {
            return InvalidImage(httpContext);
        }

        ProductAccessContext access = await ResolveAccessAsync(httpContext, userResolver, cancellationToken);
        await using Stream content = file.OpenReadStream();
        ProductImageUpload upload = new(content, file.FileName, file.ContentType, file.Length);
        Result<ProductImageResponse> result = await commandHandler.HandleAsync(
            new ManageProductImageCommand(
                productId,
                ProductImageOperation.Upload,
                access,
                Upload: upload),
            cancellationToken);

        return result.IsFailure
            ? CatalogResults.FromResult(result, httpContext)
            : Results.Created(
                $"/api/v1/products/{productId}/images/{result.Value!.Id}",
                result.Value);
    }

    private static async Task<IResult> SetMainAsync(
        Guid productId,
        Guid imageId,
        ICommandHandler<
            ManageProductImageCommand,
            Result<ProductImageResponse>> commandHandler,
        IAuthenticatedUserResolver userResolver,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        ProductAccessContext access = await ResolveAccessAsync(httpContext, userResolver, cancellationToken);
        Result<ProductImageResponse> result = await commandHandler.HandleAsync(
            new ManageProductImageCommand(
                productId,
                ProductImageOperation.SetMain,
                access,
                ImageId: imageId),
            cancellationToken);
        return CatalogResults.FromResult(result, httpContext);
    }

    private static async Task<IResult> DeleteAsync(
        Guid productId,
        Guid imageId,
        ICommandHandler<
            ManageProductImageCommand,
            Result<ProductImageResponse>> commandHandler,
        IAuthenticatedUserResolver userResolver,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        ProductAccessContext access = await ResolveAccessAsync(httpContext, userResolver, cancellationToken);
        Result<ProductImageResponse> result = await commandHandler.HandleAsync(
            new ManageProductImageCommand(
                productId,
                ProductImageOperation.Delete,
                access,
                ImageId: imageId),
            cancellationToken);
        return result.IsFailure
            ? CatalogResults.FromResult(result, httpContext)
            : Results.NoContent();
    }

    private static async Task<ProductAccessContext> ResolveAccessAsync(
        HttpContext httpContext,
        IAuthenticatedUserResolver userResolver,
        CancellationToken cancellationToken)
    {
        bool isAdmin = httpContext.User.IsInRole(ApplicationRoles.Admin);
        Guid? userId = isAdmin ? null : await userResolver.ResolveUserIdAsync(cancellationToken);
        return new ProductAccessContext(userId, isAdmin);
    }

    private static IResult InvalidImage(HttpContext httpContext)
    {
        Result<ProductImageResponse> result = Result<ProductImageResponse>.Failure(
            new Error(CatalogErrorCodes.InvalidProductImage, CatalogErrorCodes.InvalidProductImage));
        return CatalogResults.FromResult(result, httpContext);
    }
}
