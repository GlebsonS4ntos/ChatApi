using ChatApi.Dto;
using FluentValidation;

namespace ChatApi.Validations
{
    public class TokenValidation : AbstractValidator<TokenDto>
    {
        public TokenValidation() 
        {
            RuleFor(t => t.Accesstoken)
                .NotEmpty()
                .NotNull();

            RuleFor(t => t.RefreshToken)
                .NotEmpty()
                .NotNull();
        }
    }
}
