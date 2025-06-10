using ChatApi.Dto;
using FluentValidation;

namespace ChatApi.Validations
{
    public class LoginValidation : AbstractValidator<LoginDto>
    {
        public LoginValidation() 
        {
            RuleFor(u => u.UserEmail)
                .NotEmpty()
                .EmailAddress().WithMessage("E-mail inválido.");

            RuleFor(u => u.Password)
                .NotEmpty()
                .NotNull()
                .MinimumLength(6).WithMessage("A senha deve ter no mínimo 6 caracteres.")
                .MaximumLength(20).WithMessage("A senha não deve ter mais de 20 caracteres.");
        }
    }
}
