﻿/*
 * The MIT License (MIT)
 * 
 * Copyright (c) 2016-2020  Denis Kuzmin < x-3F@outlook.com > GitHub/3F
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using net.r_eg.MvsSln.Log;

namespace net.r_eg.DllExport.Wizard
{
    internal class PackageInfo
    {
        protected readonly IExecutor exec;
        private readonly string proxy;

        public string Activated => exec.Config.PkgVer;

        public Task<IEnumerable<string>> GetFromGitHubAsync()
            => GetFromRemoteAsync("https://3F.github.io/DllExport/data/pkgrel");

        public Task<IEnumerable<string>> GetFromRemoteAsync(string url)
            => RcvStringOrActivatedAsync(url)
            .ContinueWith(r => Detect301(r.Result))
            .ContinueWith(r => Regex.Matches(r.Result, @"^\s*([^#\r\n].+)$", RegexOptions.Multiline)
            .Cast<Match>()
            .Select(x => x.Groups[1].Value));

        public PackageInfo(IExecutor exec)
        {
            this.exec   = exec ?? throw new ArgumentNullException(nameof(exec));
            proxy       = exec.Config.Proxy?.Trim();
        }

        /// <summary>
        /// Emulates an emergency 301 through special command due to unsupported servers like GitHub pages.
        /// Format: `@301=url` EOF
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        protected string Detect301(string input)
        {
            const string R301 = "@301=";

            if(input?.StartsWith(R301) == false) {
                return input;
            }
            var url = input.Substring(R301.Length);

            LSender.Send(this, $"{R301}{url}", Message.Level.Debug);
            return RcvStringOrActivatedAsync(url).Result;
        }

        protected Task<string> RcvStringOrActivatedAsync(string target) 
            => RcvStringAsync(target, (ex) => Activated);

        // while we're still using .netfx 4.0
        protected Task<string> RcvStringAsync(string target, Func<Exception, string> failed)
        {
            var tcs = new TaskCompletionSource<string>();
            var url = new Uri(target);

            using(var wc = new WebClient())
            {
                if(!string.IsNullOrEmpty(proxy)) {
                    wc.Proxy = GetProxy(proxy);
                }

                wc.UseDefaultCredentials = true;
                    
                if(wc.Proxy.Credentials == null) {
                    wc.Proxy.Credentials = CredentialCache.DefaultCredentials;
                }

                long bytesReceived = 0;
                wc.DownloadStringCompleted += (sender, e) =>
                {
                    if(e.Error != null) 
                    {
                        //tcs.TrySetException(e.Error);
                        LSender.Send(this, $"Rcv failed: {e.Error.Message}", Message.Level.Debug);
                        tcs.TrySetResult(failed(e.Error));
                    }
                    else if(e.Cancelled) 
                    {
                        tcs.TrySetCanceled();
                    }
                    else 
                    {
                        tcs.TrySetResult(e.Result);
                        LSender.Send(this, $"Rcv Done: {bytesReceived} bytes.", Message.Level.Debug);
                    }
                };

                wc.DownloadProgressChanged += (sender, e) => bytesReceived = e.BytesReceived;

                LSender.Send(this, $"Get data: {url}", Message.Level.Debug);
                wc.DownloadStringAsync(url);
                return tcs.Task;
            }
        }

        private WebProxy GetProxy(string cfg)
        {
            LSender.Send(this, $"Configure proxy: {cfg}", Message.Level.Debug);

            var auth = cfg.Split('@');
            if(auth.Length <= 1)
            {
                LSender.Send(this, $"Use proxy: `{auth[0]}`", Message.Level.Debug);
                return new WebProxy(auth[0], false);
            }

            var login = auth[0].Split(':');
            LSender.Send(this, $"Use proxy: `{auth[1]}` /login: {login[0]}", Message.Level.Debug);

            return new WebProxy(auth[1], false)
            {
                Credentials = new NetworkCredential(
                    login[0],
                    (login.Length > 1) ? login[1] : null
                )
            };
        }
    }
}