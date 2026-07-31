using System.ComponentModel.DataAnnotations;

namespace MonPlan.ViewModels;

public class InscriptionViewModel
{
    [Required, EmailAddress] public string Email { get; set; } = string.Empty;
    [Required, MinLength(10), DataType(DataType.Password)] public string MotDePasse { get; set; } = string.Empty;
    [Required, DataType(DataType.Password), Compare(nameof(MotDePasse))] public string ConfirmationMotDePasse { get; set; } = string.Empty;
}

public class ConnexionViewModel
{
    [Required, EmailAddress] public string Email { get; set; } = string.Empty;
    [Required, DataType(DataType.Password)] public string MotDePasse { get; set; } = string.Empty;
    public bool SeSouvenirDeMoi { get; set; }
    public string? ReturnUrl { get; set; }
}

public class MotDePasseOublieViewModel
{
    [Required, EmailAddress] public string Email { get; set; } = string.Empty;
}

public class ReinitialiserMotDePasseViewModel
{
    [Required, EmailAddress] public string Email { get; set; } = string.Empty;
    [Required] public string Code { get; set; } = string.Empty;
    [Required, MinLength(10), DataType(DataType.Password)] public string MotDePasse { get; set; } = string.Empty;
    [Required, DataType(DataType.Password), Compare(nameof(MotDePasse))] public string ConfirmationMotDePasse { get; set; } = string.Empty;
}
