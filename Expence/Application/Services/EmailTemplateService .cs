using System.Drawing;
using static System.Net.Mime.MediaTypeNames;

namespace Expence.Application.Services
{
    public class EmailTemplateService
    {
        public static string GetPasswordResetEmailTemplate(string userEmail, string resetToken, string frontendUrl)
        {
            var resetLink = $"{frontendUrl}/reset-password?email={Uri.EscapeDataString(userEmail)}&token={Uri.EscapeDataString(resetToken)}";

            return $$"""
            <!DOCTYPE html>
            <html lang="en">
            <head>
                <meta charset="UTF-8">
                <meta name="viewport" content="width=device-width, initial-scale=1.0">
                <title>Password Reset</title>
                <style>
                    body {
                        font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
                        line-height: 1.6;
                        color: #333;
                        margin: 0;
                        padding: 0;
                        background-color: #f5f5f5;
                    }
                    .container {
                        max-width: 600px;
                        margin: 20px auto;
                        padding: 0;
                        background-color: #ffffff;
                        border-radius: 8px;
                        overflow: hidden;
                        box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
                    }
                    .header {
                        background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
                        color: white;
                        padding: 30px 20px;
                        text-align: center;
                    }
                    .header h1 {
                        margin: 0;
                        font-size: 28px;
                    }
                    .content {
                        padding: 30px 20px;
                    }
                    .content p {
                        margin: 10px 0;
                    }
                    .reset-section {
                        background-color: #f9f9f9;
                        padding: 20px;
                        border-radius: 5px;
                        margin: 20px 0;
                        text-align: center;
                    }
                    .reset-btn {
                        display: inline-block;
                        background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
                        color: white;
                        padding: 12px 30px;
                        text-decoration: none;
                        border-radius: 5px;
                        font-weight: bold;
                        margin: 15px 0;
                    }
                    .reset-btn:hover {
                        opacity: 0.9;
                    }
                    .token-section {
                        background-color: #f0f0f0;
                        padding: 15px;
                        border-radius: 5px;
                        margin: 15px 0;
                        word-break: break-all;
                        font-family: monospace;
                        font-size: 12px;
                    }
                    .footer {
                        background-color: #f5f5f5;
                        padding: 20px;
                        text-align: center;
                        font-size: 12px;
                        color: #999;
                        border-top: 1px solid #eee;
                    }
                    .warning {
                        color: #d32f2f;
                        font-size: 12px;
                        margin-top: 10px;
                    }   
                </style>
            </head>
            <body>
                <div class="container">
                    <div class="header">
                        <h1>Reset Your Password</h1>
                    </div>
                    <div class="content">
                        <p>Hello,</p>
                        <p>We received a request to reset your password for your Expence account. Click the button below to reset your password:</p>
                        
                        <div class="reset-section">
                            <a href="{resetLink}" class="reset-btn">Reset Password</a>
                            <p style="margin: 10px 0 0 0; font-size: 12px; color: #666;">or copy this link:</p>
                            <p style="word-break: break-all; font-size: 11px; color: #666; margin: 5px 0;">{resetLink}</p>
                        </div>

                        <p><strong>Or use this reset token:</strong></p>
                        <div class="token-section">
                            {resetToken}
                        </div>

                        <p style="color: #666; font-size: 14px;">This password reset link will expire in <strong>1 hour</strong>.</p>
                        
                        <p class="warning">⚠️ If you didn't request a password reset, please ignore this email or contact our support team.</p>
                        
                        <p>Best regards,<br>The Expence Team</p>
                    </div>
                    <div class="footer">
                        <p>&copy; 2024 Expence. All rights reserved.</p>
                        <p>If you have any questions, please contact us at support@expence.app</p>
                    </div>
                </div>
            </body>
            </html>
            """;
        }

        public static string GetWelcomeEmailTemplate(string userEmail)
        {
            return $$"""
            <!DOCTYPE html>
            <html lang="en">
            <head>
                <meta charset="UTF-8">
                <meta name="viewport" content="width=device-width, initial-scale=1.0">
                <title>Welcome to Expence</title>
                <style>
                    body {
                        font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
                        line-height: 1.6;
                        color: #333;
                        margin: 0;
                        padding: 0;
                        background-color: #f5f5f5;
                    }
                    .container {
                        max-width: 600px;
                        margin: 20px auto;
                        padding: 0;
                        background-color: #ffffff;
                        border-radius: 8px;
                        overflow: hidden;
                        box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
                    }
                    .header {
                        background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
                        color: white;
                        padding: 30px 20px;
                        text-align: center;
                    }
                    .header h1 {
                        margin: 0;
                        font-size: 28px;
                    }
                    .content {
                        padding: 30px 20px;
                    }
                    .footer {
                        background-color: #f5f5f5;
                        padding: 20px;
                        text-align: center;
                        font-size: 12px;
                        color: #999;
                        border-top: 1px solid #eee;
                    }
                </style>
            </head>
            <body>
                <div class="container">
                    <div class="header">
                        <h1>Welcome to Expence!</h1>
                    </div>
                    <div class="content">
                        <p>Hello,</p>
                        <p>Your account has been created successfully! We're excited to have you on board.</p>
                        <p>With Expence, you can:</p>
                        <ul>
                            <li>Track your income and expenses effortlessly</li>
                            <li>Get AI-powered category predictions for your transactions</li>
                            <li>Receive intelligent insights about your spending patterns</li>
                            <li>Generate detailed monthly expense reports</li>
                        </ul>
                        <p>Get started today and take control of your finances!</p>
                        <p>Best regards,<br>The Expence Team</p>
                    </div>
                    <div class="footer">
                        <p>&copy; 2024 Expence. All rights reserved.</p>
                        <p>If you have any questions, please contact us at support@expence.app</p>
                    </div>
                </div>
            </body>
            </html>
            """;
        }

        // Add this method to existing EmailTemplateService class

        public static string GetEmailVerificationTemplate(string userEmail, string verificationToken, string frontendUrl)
        {
            var verificationLink = $"{frontendUrl}/verify-email?email={Uri.EscapeDataString(userEmail)}&token={Uri.EscapeDataString(verificationToken)}";

            return $$"""
    <!DOCTYPE html>
    <html lang="en">
    <head>
        <meta charset="UTF-8">
        <meta name="viewport" content="width=device-width, initial-scale=1.0">
        <title>Verify Your Email</title>
        <style>
            body {
                font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
                line-height: 1.6;
                color: #333;
                margin: 0;
                padding: 0;
                background-color: #f5f5f5;
            }
            .container {
                max-width: 600px;
                margin: 20px auto;
                padding: 0;
                background-color: #ffffff;
                border-radius: 8px;
                overflow: hidden;
                box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
            }
            .header {
                background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
                color: white;
                padding: 30px 20px;
                text-align: center;
            }
            .header h1 {
                margin: 0;
                font-size: 28px;
            }
            .content {
                padding: 30px 20px;
            }
            .content p {
                margin: 10px 0;
            }
            .verify-section {
                background-color: #f9f9f9;
                padding: 20px;
                border-radius: 5px;
                margin: 20px 0;
                text-align: center;
            }
            .verify-btn {
                display: inline-block;
                background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
                color: white;
                padding: 12px 30px;
                text-decoration: none;
                border-radius: 5px;
                font-weight: bold;
                margin: 15px 0;
            }
            .verify-btn:hover {
                opacity: 0.9;
            }
            .token-section {
                background-color: #f0f0f0;
                padding: 15px;
                border-radius: 5px;
                margin: 15px 0;
                word-break: break-all;
                font-family: monospace;
                font-size: 12px;
            }
            .footer {
                background-color: #f5f5f5;
                padding: 20px;
                text-align: center;
                font-size: 12px;
                color: #999;
                border-top: 1px solid #eee;
            }
            .warning {
                color: #d32f2f;
                font-size: 12px;
                margin-top: 10px;
            }
            .timer {
                color: #ff9800;
                font-weight: bold;
                margin-top: 10px;
            }
        </style>
    </head>
    <body>
        <div class="container">
            <div class="header">
                <h1>Verify Your Email</h1>
            </div>
            <div class="content">
                <p>Hello,</p>
                <p>Thank you for registering with Expence! To complete your account setup and gain full access, please verify your email address by clicking the button below:</p>
                
                <div class="verify-section">
                    <a href="{verificationLink}" class="verify-btn">Verify Email</a>
                    <p style="margin: 10px 0 0 0; font-size: 12px; color: #666;">or copy this link:</p>
                    <p style="word-break: break-all; font-size: 11px; color: #666; margin: 5px 0;">{verificationLink}</p>
                </div>

                <p><strong>Or use this verification token:</strong></p>
                <div class="token-section">
                    {verificationToken}
                </div>

                <p style="color: #666; font-size: 14px;">
                    This verification link will expire in <strong>24 hours</strong>.
                </p>
                <div class="timer">
                    ⏱️ Verification expires in 24 hours. Don't wait!
                </div>

                <p class="warning">⚠️ If you didn't create this account, please disregard this email and contact our support team.</p>
                
                <p>After verification, you'll be able to:</p>
                <ul>
                    <li>Access your account with full features</li>
                    <li>Create and manage transactions</li>
                    <li>Use AI-powered expense categorization</li>
                    <li>Receive important security notifications</li>
                </ul>
                
                <p>Best regards,<br>The Expence Team</p>
            </div>
            <div class="footer">
                <p>&copy; 2024 Expence. All rights reserved.</p>
                <p>If you have any questions, please contact us at support@expence.app</p>
            </div>
        </div>
    </body>
    </html>
    """;
        }
    }
}
