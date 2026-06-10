ABP Development Standards
General Rules
•	Keep methods small and focused on a single responsibility.
•	Extract private methods when a method becomes large or difficult to read.
________________________________________
Entities
•	Use static factory methods for entity creation.
•	Do not expose public setters on entity properties.
•	Entity state changes must be performed through domain methods.
•	Validate business rules inside entities and managers.
•	Use GuidGenerator.Create() instead of Guid.NewGuid().
•	Keep entities persistence-ignorant as much as possible.
Example:
public class Recommendation : FullAuditedAggregateRoot<Guid>
{
    public string RecommendationText { get; private set; }

    private Recommendation()
    {
    }

    public static Recommendation Create(
        Guid id,
        string recommendationText)
    {
        return new Recommendation
        {
            Id = id,
            RecommendationText = recommendationText
        };
    }

    public void UpdateText(string recommendationText)
    {
        RecommendationText = recommendationText;
    }
}
________________________________________
Constants
•	Create an EntityNameConsts class for every entity in the Domain.Shared layer.
•	Store:
o	MinLength
o	MaxLength
o	Regex patterns
o	Default values
o	Business constants
•	Reuse these constants in:
o	EF Core configurations
o	FluentValidation validators
o	DTO attributes
o	ui validation
o	Domain validation
Example:
public static class RecommendationConsts
{
    public const int RecommendationTextMinLength = 10;
    public const int RecommendationTextMaxLength = 1000;
}
________________________________________
DTOs
•	All DTOs must be declared as sealed record.
•	DTOs should not contain business logic.
•	Never expose entities outside the domain layer.
•	Application services should return DTOs only.
Example:
public sealed record CreateRecommendationDto(
    string RecommendationText,
    Guid RecommendedUserId);
________________________________________
Validation
•	Use FluentValidation.
•	Create one validator per DTO.
•	All validators must be sealed.
•	Validators should inherit from AbstractValidator<TDto>.
•	Perform validation at the beginning of application service methods.
Example:
public sealed class CreateRecommendationDtoValidator
    : AbstractValidator<CreateRecommendationDto>
{
    public CreateRecommendationDtoValidator()
    {
        RuleFor(x => x.RecommendationText)
            .NotEmpty()
            .MinimumLength(RecommendationConsts.RecommendationTextMinLength)
            .MaximumLength(RecommendationConsts.RecommendationTextMaxLength);
    }
}
________________________________________
Result Pattern
Application Layer
•	Application service methods must return ResultDto<T>.
Example:
Task<ResultDto<RecommendationDto>> CreateAsync(
    CreateRecommendationDto input);
Domain Layer
•	Manager methods must return or Result.
Example:
Task<Result<Recommendation>> CreateAsync(
    string recommendationText,
    Guid recommendedUserId,
    CancellationToken cancellationToken = default);
________________________________________
Managers
•	All business logic belongs to managers and entities.
•	Do not place business logic in application services.
•	All entity creation should be performed through managers.
•	Managers should be responsible for business rule validation.
________________________________________
Application Services
Application services should only:
1.	Validate input.
2.	Call manager/domain service.
3.	Map results.
4.	Return DTOs.
Avoid:
•	Repository access when a manager exists.
•	Business logic.
•	Complex validation logic.
________________________________________
Repositories
•	Access repositories only from managers/domain services when business logic exists.
•	Avoid duplicating query logic.
________________________________________
Mapping
•	Use ObjectMapper only in Application Services.
•	Do not inject ObjectMapper into managers.
•	Do not perform DTO mapping inside the domain layer.
________________________________________
Error Handling
•	Use centralized domain error codes.
•	Avoid hardcoded error messages.
Example:
public static class RecommendationErrorCodes
{
    public const string RecommendationAlreadyExists =
        "Recommendation:00001";
}
________________________________________
Permissions
•	Create a permissions class for every module.
•	Never hardcode permission names.
Example:
public static class RecommendationPermissions
{
    public const string Default = "Recommendation";
    public const string Create = "Recommendation.Create";
    public const string Edit = "Recommendation.Edit";
}
________________________________________
Cancellation Tokens
•	Pass CancellationToken through all async methods.
•	ReuseCancellationToken to all lower-level calls like repositories, database operations, and external services so they can stop work when cancellation is requested.Example:

Task<Result> CreateAsync(
    CreateRecommendationDto input,
    CancellationToken cancellationToken = default);
________________________________________
Code Quality
•	Keep classes focused and cohesive.
•	Follow consistent naming conventions.
•	Avoid duplicated code.
•	Use meaningful method and variable names.
•	Maintain a single source of truth for validation and business rules.
