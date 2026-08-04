using ECommerce.BuildingBlocks.Contracts.Errors;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.BuildingBlocks.Security;
using ECommerce.Catalog.Application;
using ECommerce.Catalog.Application.Commands.ManageProductImage;
using ECommerce.Catalog.Application.Images;
using ECommerce.Catalog.Application.Products;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Catalog.Api.Products;

[ApiController]
[Route("api/v1/products/{productId:guid}/images")]
[Authorize(Policy = AuthorizationPolicies.SellerOrAdmin)]
public sealed class ProductImagesController(
    ISender sender,
    IAuthenticatedUserResolver userResolver) : ControllerBase
{
    [HttpPost(Name = "UploadProductImage")]
    [Consumes("multipart/form-data")]
    [IgnoreAntiforgeryToken]
    public async Task<IResult> UploadAsync(
        Guid productId,
        CancellationToken cancellationToken)
    {
        if (!Request.HasFormContentType ||
            Request.ContentLength > ProductImageUploadValidator.MaxFileSize + (64 * 1024))
        {
            return InvalidImage();
        }

        IFormCollection form;
        try
        {
            form = await Request.ReadFormAsync(cancellationToken);
        }
        catch (InvalidDataException)
        {
            return InvalidImage();
        }

        IFormFile? file = form.Files.GetFile("file");
        if (file is null)
        {
            return InvalidImage();
        }

        ProductAccessContext access = await ResolveAccessAsync(cancellationToken);
        await using Stream content = file.OpenReadStream();
        ProductImageUpload upload = new(content, file.FileName, file.ContentType, file.Length);
        Result<ProductImageResponse> result = await sender.Send(
            new ManageProductImageCommand(
                productId,
                ProductImageOperation.Upload,
                access,
                Upload: upload),
            cancellationToken);

        return result.IsFailure
            ? CatalogResults.FromResult(result, HttpContext)
            : Results.Created(
                $"/api/v1/products/{productId}/images/{result.Value!.Id}",
                result.Value);
    }

    [HttpPut("{imageId:guid}/main", Name = "SetMainProductImage")]
    public async Task<IResult> SetMainAsync(
        Guid productId,
        Guid imageId,
        CancellationToken cancellationToken)
    {
        ProductAccessContext access = await ResolveAccessAsync(cancellationToken);
        Result<ProductImageResponse> result = await sender.Send(
            new ManageProductImageCommand(
                productId,
                ProductImageOperation.SetMain,
                access,
                ImageId: imageId),
            cancellationToken);
        return CatalogResults.FromResult(result, HttpContext);
    }

    [HttpDelete("{imageId:guid}", Name = "DeleteProductImage")]
    public async Task<IResult> DeleteAsync(
        Guid productId,
        Guid imageId,
        CancellationToken cancellationToken)
    {
        ProductAccessContext access = await ResolveAccessAsync(cancellationToken);
        Result<ProductImageResponse> result = await sender.Send(
            new ManageProductImageCommand(
                productId,
                ProductImageOperation.Delete,
                access,
                ImageId: imageId),
            cancellationToken);
        return result.IsFailure
            ? CatalogResults.FromResult(result, HttpContext)
            : Results.NoContent();
    }

    private async Task<ProductAccessContext> ResolveAccessAsync(
        CancellationToken cancellationToken)
    {
        bool isAdmin = User.IsInRole(ApplicationRoles.Admin);
        Guid? userId = isAdmin
            ? null
            : await userResolver.ResolveUserIdAsync(cancellationToken);
        return new ProductAccessContext(userId, isAdmin);
    }

    private IResult InvalidImage()
    {
        Result<ProductImageResponse> result = Result<ProductImageResponse>.Failure(
            new Error(
                CatalogErrorCodes.InvalidProductImage,
                CatalogErrorCodes.InvalidProductImage));
        return CatalogResults.FromResult(result, HttpContext);
    }
}
