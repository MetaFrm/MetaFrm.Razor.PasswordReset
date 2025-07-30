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
    public partial class PasswordReset : IDisposable
    {
        private PasswordResetViewModel PasswordResetViewModel { get; set; } = new(null);
        private bool _isFocusElement = false;//등록 버튼 클릭하고 AccessCode로 포커스가 한번만 가도록
        private TimeSpan RemainingTimeOrg { get; set; } = new TimeSpan(0, 5, 0);
        private TimeSpan RemainingTime { get; set; }
        private bool IsLoadAutoFocus;

        /// <summary>
        /// OnInitialized
        /// </summary>
        protected override void OnInitialized()
        {
            base.OnInitialized();

            this.PasswordResetViewModel = this.CreateViewModel<PasswordResetViewModel>();

            try
            {
                string[] time = this.GetAttribute(nameof(this.RemainingTime)).Split(":");
                this.RemainingTimeOrg = new TimeSpan(time[0].ToInt(), time[1].ToInt(), time[2].ToInt());

                this.RemainingTime = new TimeSpan(this.RemainingTimeOrg.Ticks);
                this.IsLoadAutoFocus = this.GetAttributeBool(nameof(this.IsLoadAutoFocus));
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// OnAfterRender
        /// </summary>
        /// <param name="firstRender"></param>
        protected override void OnAfterRender(bool firstRender)
        {
            base.OnAfterRender(firstRender);

            if (firstRender)
            {
                //if (this.AuthState.IsLogin())
                //    this.Navigation?.NavigateTo("/", true);

                if (this.IsLoadAutoFocus)
                {
                    ValueTask? _ = this.JSRuntime?.InvokeVoidAsync("ElementFocus", "email");
                }
            }

            if (this.PasswordResetViewModel.AccessCodeVisible && !this._isFocusElement)
            {
                ValueTask? _ = this.JSRuntime?.InvokeVoidAsync("ElementFocus", "inputaccesscode");
                this._isFocusElement = true;
            }
        }

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        /// <summary>
        /// Dispose
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                try
                {
                    this.timer.Stop();
                    this.timer.Elapsed -= Timer_Elapsed;
                    this.timer.Dispose();
                }
                catch (Exception) { }
            }
        }

        private async Task<bool> OnPasswordResetClick()
        {
            Response response;

            if (this.PasswordResetViewModel.IsBusy) return false;

            try
            {
                this.PasswordResetViewModel.IsBusy = true;

                if (!this.PasswordResetViewModel.Email.IsNullOrEmpty()
                    && this.PasswordResetViewModel.Password != null && !this.PasswordResetViewModel.Password.IsNullOrEmpty() && !this.PasswordResetViewModel.RepeatPassword.IsNullOrEmpty()
                    && !this.PasswordResetViewModel.InputAccessCode.IsNullOrEmpty() && this.PasswordResetViewModel.AccessCode == this.PasswordResetViewModel.InputAccessCode)
                {
                    ServiceData serviceData = new()
                    {
                        TransactionScope = true,
                        Token = this.AuthState.IsLogin() ? this.AuthState.Token() : Factory.AccessKey
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

                        this.ToastShow("암호 재설정", "비밀번호 재설정이 완료되었습니다.", ToastDuration.Long);

                        if (!this.AuthState.IsLogin())
                            this.OnAction(this, new MetaFrmEventArgs { Action = "Login" });
                        else
                            this.TimerStop();

                        return true;
                    }
                    else
                    {
                        if (response.Message != null)
                        {
                            this.ModalShow("암호 재설정", response.Message, new() { { "Ok", Btn.Warning } }, EventCallback.Factory.Create<string>(this, OnClickFunctionAsync));
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
            ValueTask? _ = this.JSRuntime?.InvokeVoidAsync("ElementFocus", "inputaccesscode");
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
                this.RemainingTime = this.RemainingTime.Add(new TimeSpan(0, 0, -1));

                if (this.RemainingTime.Ticks <= 0)
                    this.TimerStop();

                this.InvokeAsync(this.StateHasChanged);
            }
            catch (Exception)
            {
            }
        }
        private void TimerStop()
        {
            this.PasswordResetViewModel.AccessCodeVisible = false;
            this._isFocusElement = true;
            this.PasswordResetViewModel.AccessCode = null;
            this.PasswordResetViewModel.InputAccessCode = null;
            this.PasswordResetViewModel.AccessCodeConfirmVisible = false;
            this.RemainingTime = new TimeSpan(this.RemainingTimeOrg.Ticks);
            this.timer.Stop();
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
                ValueTask? _ = this.JSRuntime?.InvokeVoidAsync("ElementFocus", "inputaccesscode");
            }
        }
        private void InputAccessCodeKeydown(KeyboardEventArgs args)
        {
            if (args.Key == "Enter" && this.PasswordResetViewModel.AccessCodeVisible && this.PasswordResetViewModel.AccessCode == this.PasswordResetViewModel.InputAccessCode)
            {
                ValueTask? _ = this.JSRuntime?.InvokeVoidAsync("ElementFocus", "password");
            }
        }

        private void PasswordKeydown(KeyboardEventArgs args)
        {
            if (args.Key == "Enter" && !this.PasswordResetViewModel.Password.IsNullOrEmpty())
            {
                ValueTask? _ = this.JSRuntime?.InvokeVoidAsync("ElementFocus", "repeatpassword");
            }
        }
    }
}