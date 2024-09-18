﻿namespace Contracts.Configurations;

public interface IEmailSMTPSettings
{
    public string DisplayName { get; set; }
    public bool EnableVerification { get; set; }
    public string From { get; set; }
    public string SMTPServer { get; set; }
    public bool UseSsl { get; set; }
    public int Port { get; set; }
    public string UserName { get; set; }
    public string Password { get; set; }
}