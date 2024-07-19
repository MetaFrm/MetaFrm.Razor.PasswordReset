using MetaFrm.MVVM;
using MetaFrm.Razor.Essentials.ComponentModel.DataAnnotations;
using DisplayAttribute = System.ComponentModel.DataAnnotations.DisplayAttribute;

namespace MetaFrm.Razor.ViewModels
{
    /// <summary>
    /// RegisterViewModel
    /// </summary>
    public partial class PasswordResetViewModel : BaseViewModel
    {
        /// <summary>
        /// Email
        /// </summary>
        [Display(Name = "이메일")]
        [Required]
        [EmailAddress]
        public string? Email { get; set; }

        /// <summary>
        /// Password
        /// </summary>
        [Display(Name = "비밀번호")]
        [Required]
        [MinLength(6)]
        public string? Password { get; set; }

        /// <summary>
        /// RepeatPassword
        /// </summary>
        [Display(Name = "비밀번호 반복")]
        [Required]
        [CompareAttribute("Password")]
        [MinLength(6)]
        public string? RepeatPassword { get; set; }

        /// <summary>
        /// AccessCodeVisible
        /// </summary>
        public bool AccessCodeVisible { get; set; }

        /// <summary>
        /// AccessCode
        /// </summary>
        public string? AccessCode { get; set; }

        private string? _inputAccessCode;
        /// <summary>
        /// InputAccessCode
        /// </summary>
        public string? InputAccessCode
        {
            get
            {
                return this._inputAccessCode;
            }
            set
            {
                this._inputAccessCode = value;

                this.AccessCodeConfirmVisible = this._inputAccessCode == this.AccessCode;

            }
        }

        /// <summary>
        /// AccessCodeConfirmVisible
        /// </summary>
        public bool AccessCodeConfirmVisible { get; set; }
    }
}