using MetaFrm.MVVM;
using System.ComponentModel.DataAnnotations;

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
        [Required]
        [EmailAddress]
        public string? Email { get; set; }

        /// <summary>
        /// Password
        /// </summary>
        [Required]
        [MinLength(6)]
        public string? Password { get; set; }

        /// <summary>
        /// RepeatPassword
        /// </summary>
        [Required]
        [CompareAttribute("Password")]
        [Display(Name = "Repeat Password")]
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