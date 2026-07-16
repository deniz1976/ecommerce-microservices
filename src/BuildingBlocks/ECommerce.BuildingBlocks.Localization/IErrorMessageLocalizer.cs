namespace ECommerce.BuildingBlocks.Localization;

public interface IErrorMessageLocalizer
{
    string GetMessage(string code, string? culture);
}
