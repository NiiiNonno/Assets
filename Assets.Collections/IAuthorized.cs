namespace Nonno.Assets.Collections
{
    public interface IAuthorized
    {
        Authority Authority => Authority.Unknown;
    }

    public interface IAuthorized<I> : IAuthorized
    {
        new Authority<I> Authority { get; }
        I GetInterface();

        Authority IAuthorized.Authority => Authority;
    }
}