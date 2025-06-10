using ChatApi.Dto;
using FluentValidation;

namespace ChatApi.Validations
{
    public class CreateUserValidation : AbstractValidator<CreateUserDto>
    {
        public CreateUserValidation() 
        { 
            RuleFor(u => u.UserEmail)
                .NotEmpty()
                .EmailAddress().WithMessage("E-mail inválido.");

            RuleFor(u => u.UserName)
                .NotEmpty()
                .NotNull()
                .MinimumLength(4).WithMessage("O username deve ter no mínimo 4 caracteres.")
                .MaximumLength(20).WithMessage("O username deve ter no máximo 20 caracteres.");

            RuleFor(u => u.Password)
                .NotEmpty()
                .NotNull()
                .MinimumLength(6).WithMessage("A senha deve ter no mínimo 6 caracteres.")
                .MaximumLength(20).WithMessage("A senha não deve ter mais de 20 caracteres.");
        }
    }
}
