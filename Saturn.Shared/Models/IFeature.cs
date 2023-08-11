namespace Saturn.BL.FeatureUtils
{
    public interface IFeature
    {
        Guid Id { get; }
        Type Type { get; }
        Action Program { get; }
        bool isEnabled { get; set; }

        void Enable();
        void Disable();
        Task Run();
    }
}
