using MetaFrm.Alert;
using MetaFrm.Control;
using MetaFrm.Service;
using MetaFrm.Web.Bootstrap;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Timers;
using MetaFrm.Razor.ViewModels;
using MetaFrm.Database;

namespace MetaFrm.Razor
{
    /// <summary>
    /// PasswordReset
    /// </summary>
    public partial class PasswordReset
    {
        internal PasswordResetViewModel PasswordResetViewModel { get; set; } = new PasswordResetViewModel();

        private bool _isFocusElement = false;//등록 버튼 클릭하고 AccessCode로 포커스가 한번만 가도록

        private TimeSpan RemainTimeOrg { get; set; } = new TimeSpan(0, 5, 0);

        private TimeSpan RemainTime { get; set; }

        Auth.AuthenticationStateProvider AuthenticationState;

        /// <summary>
        /// OnInitialized
        /// </summary>
        protected override void OnInitialized()
        {
            this.AuthenticationState ??= (this.AuthStateProvider as Auth.AuthenticationStateProvider) ?? (Auth.AuthenticationStateProvider)Factory.CreateInstance(typeof(Auth.AuthenticationStateProvider));

            try
            {
                string[] time = this.GetAttribute("RemainingTime").Split(":");

                this.RemainTimeOrg = new TimeSpan(time[0].ToInt(), time[1].ToInt(), time[2].ToInt());

                this.RemainTime = new TimeSpan(this.RemainTimeOrg.Ticks);
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// OnAfterRender
        /// </summary>
        /// <param name="firstRender"></param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2012:올바르게 ValueTasks 사용", Justification = "<보류 중>")]
        protected override void OnAfterRender(bool firstRender)
        {
            base.OnAfterRender(firstRender);

            if (firstRender)
            {
                if (this.AuthenticationState.IsLogin())
                    this.Navigation?.NavigateTo("/", true);

                this.JSRuntime?.InvokeVoidAsync("ElementFocus", "email");
            }

            if (this.PasswordResetViewModel.AccessCodeVisible && !this._isFocusElement)
            {
                this.JSRuntime?.InvokeVoidAsync("ElementFocus", "inputaccesscode");
                this._isFocusElement = true;
            }
        }

        private async Task<bool> OnPasswordResetClick()
        {
            try
            {
                this.PasswordResetViewModel.IsBusy = true;

                if (!this.AuthenticationState.IsLogin())
                {
                    Response response;


                    if (!this.PasswordResetViewModel.Email.IsNullOrEmpty() 
                        && this.PasswordResetViewModel.Password != null && !this.PasswordResetViewModel.Password.IsNullOrEmpty() && !this.PasswordResetViewModel.RepeatPassword.IsNullOrEmpty() 
                        && !this.PasswordResetViewModel.InputAccessCode.IsNullOrEmpty() && this.PasswordResetViewModel.AccessCode == this.PasswordResetViewModel.InputAccessCode)
                    {
                        ServiceData serviceData = new()
                        {
                            TransactionScope = true,
                            Token = Factory.AccessKey
                        };
                        serviceData["1"].CommandText = this.GetAttribute("PasswordReset");
                        serviceData["1"].AddParameter("EMAIL", DbType.NVarChar, 100, this.PasswordResetViewModel.Email);
                        serviceData["1"].AddParameter("ACCESS_NUMBER", DbType.NVarChar, 4000, this.PasswordResetViewModel.Password.ComputeHash());
                        serviceData["1"].AddParameter("ACCESS_CODE", DbType.NVarChar, 10, this.PasswordResetViewModel.InputAccessCode);

                        response = await this.ServiceRequestAsync(serviceData);

                        if (response.Status == Status.OK)
                        {
                            this.PasswordResetViewModel.Password = string.Empty;
                            this.PasswordResetViewModel.RepeatPassword = string.Empty;

                            this.ToastShow("Password Reset", "Password reset is complete.", ToastDuration.Long);

                            this.OnAction(this, new MetaFrmEventArgs { Action = "Login" });
                            return true;
                        }
                        else
                        {
                            if (response.Message != null)
                            {
                                this.ModalShow("Password Reset", response.Message, new() { { "Ok", Btn.Warning } }, EventCallback.Factory.Create<string>(this, OnClickFunctionAsync));
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                this.ModalShow("An Exception has occurred.", e.Message, new() { { "Ok", Btn.Danger } }, null);
            }
            finally
            {
                this.PasswordResetViewModel.IsBusy = false;
            }

            return false;
        }
        private async Task OnClickFunctionAsync(string action)
        {
            await Task.Delay(100);
#pragma warning disable CA2012 // 올바르게 ValueTasks 사용
            this.JSRuntime?.InvokeVoidAsync("ElementFocus", "inputaccesscode");
#pragma warning restore CA2012 // 올바르게 ValueTasks 사용
        }


        private readonly System.Timers.Timer timer = new(1000);
        private async void GetAccessCode()
        {
            try
            {

                if (this.PasswordResetViewModel.Email != null && !this.PasswordResetViewModel.AccessCodeVisible)
                {
                    this.PasswordResetViewModel.AccessCode = await this.JoinAccessCodeServiceRequestAsync(this.PasswordResetViewModel.Email);
                    this._isFocusElement = false;
                    this.PasswordResetViewModel.AccessCodeVisible = true;

                    try
                    {
                        this.timer.Elapsed -= Timer_Elapsed;
                    }
                    catch (Exception)
                    {
                    }
                    this.timer.Elapsed += Timer_Elapsed;
                    this.timer.Start();
                }
            }
            catch (Exception)
            {
            }
        }

        private void Timer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            try
            {
                this.RemainTime = this.RemainTime.Add(new TimeSpan(0, 0, -1));

                if (this.RemainTime.Ticks <= 0)
                {
                    this.PasswordResetViewModel.AccessCodeVisible = false;
                    this._isFocusElement = true;
                    this.PasswordResetViewModel.AccessCode = null;
                    this.PasswordResetViewModel.InputAccessCode = null;
                    this.PasswordResetViewModel.AccessCodeConfirmVisible = false;
                    this.RemainTime = new TimeSpan(this.RemainTimeOrg.Ticks);
                    this.timer.Stop();
                }

                this.InvokeStateHasChanged();
            }
            catch (Exception)
            {
            }
        }
        private void HandleInvalidSubmit(EditContext context)
        {
            //this.PasswordResetViewModel.AccessCodeVisible = false;
        }
        private void EmailKeydown(KeyboardEventArgs args)
        {
            if (args.Key == "Enter" && !this.PasswordResetViewModel.Email.IsNullOrEmpty())
            {
                this.GetAccessCode();
#pragma warning disable CA2012 // 올바르게 ValueTasks 사용
                this.JSRuntime?.InvokeVoidAsync("ElementFocus", "inputaccesscode");
#pragma warning restore CA2012 // 올바르게 ValueTasks 사용
            }
        }
        private void InputAccessCodeKeydown(KeyboardEventArgs args)
        {
            if (args.Key == "Enter" && this.PasswordResetViewModel.AccessCodeVisible && this.PasswordResetViewModel.AccessCode == this.PasswordResetViewModel.InputAccessCode)
            {
#pragma warning disable CA2012 // 올바르게 ValueTasks 사용
                this.JSRuntime?.InvokeVoidAsync("ElementFocus", "password");
#pragma warning restore CA2012 // 올바르게 ValueTasks 사용
            }
        }

        private void PasswordKeydown(KeyboardEventArgs args)
        {
            if (args.Key == "Enter" && !this.PasswordResetViewModel.Password.IsNullOrEmpty())
            {
#pragma warning disable CA2012 // 올바르게 ValueTasks 사용
                this.JSRuntime?.InvokeVoidAsync("ElementFocus", "repeatpassword");
#pragma warning restore CA2012 // 올바르게 ValueTasks 사용
            }
        }
    }
}