namespace FCG.RegoCms
{
    public class ValidationResult
    {
        public bool                 IsValid { get; internal set; }
        public ValidationError[]    ValidationErrors { get; internal set; }
    }

    public class ValidationError
    {
        public string Message { get; internal set; }
        public string Field { get; internal set; }
    }
}