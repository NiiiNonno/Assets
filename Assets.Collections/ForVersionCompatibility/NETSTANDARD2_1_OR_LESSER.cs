#if NETSTANDARD

[System.AttributeUsage(AttributeTargets.Parameter, Inherited = false, AllowMultiple = false)]
internal sealed class CallerArgumentExpressionAttribute : Attribute
{
    // See the attribute guidelines at 
    //  http://go.microsoft.com/fwlink/?LinkId=85236
    readonly string positionalString;

    // This is a positional argument
    public CallerArgumentExpressionAttribute(string positionalString)
    {
        this.positionalString = positionalString;

        // TODO: Implement code here

        throw new NotImplementedException();
    }

    public string PositionalString
    {
        get { return positionalString; }
    }

    // This is a named argument
    public int NamedInt { get; set; }
}

#endif