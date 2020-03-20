namespace System
{
    internal static partial class SR
    {
        private static global::System.Resources.ResourceManager? s_resourceManager;
        internal static global::System.Resources.ResourceManager ResourceManager => s_resourceManager ??= new global::System.Resources.ResourceManager(typeof(System.Net.Quic.Resources.Strings));

        /// <summary>The requested security protocol is not supported.</summary>
        internal static string @net_securityprotocolnotsupported => GetResourceString("net_securityprotocolnotsupported", @"The requested security protocol is not supported.");
        /// <summary>The format of the HTTP method is invalid.</summary>
        internal static string @net_http_httpmethod_format_error => GetResourceString("net_http_httpmethod_format_error", @"The format of the HTTP method is invalid.");
        /// <summary>The HTTP method '{0}' is not supported on this platform.</summary>
        internal static string @net_http_httpmethod_notsupported_error => GetResourceString("net_http_httpmethod_notsupported_error", @"The HTTP method '{0}' is not supported on this platform.");
        /// <summary>The reason phrase must not contain new-line characters.</summary>
        internal static string @net_http_reasonphrase_format_error => GetResourceString("net_http_reasonphrase_format_error", @"The reason phrase must not contain new-line characters.");
        /// <summary>The number of elements is greater than the available space from arrayIndex to the end of the destination array.</summary>
        internal static string @net_http_copyto_array_too_small => GetResourceString("net_http_copyto_array_too_small", @"The number of elements is greater than the available space from arrayIndex to the end of the destination array.");
        /// <summary>The given header was not found.</summary>
        internal static string @net_http_headers_not_found => GetResourceString("net_http_headers_not_found", @"The given header was not found.");
        /// <summary>Cannot add value because header '{0}' does not support multiple values.</summary>
        internal static string @net_http_headers_single_value_header => GetResourceString("net_http_headers_single_value_header", @"Cannot add value because header '{0}' does not support multiple values.");
        /// <summary>The header name format is invalid.</summary>
        internal static string @net_http_headers_invalid_header_name => GetResourceString("net_http_headers_invalid_header_name", @"The header name format is invalid.");
        /// <summary>The format of value '{0}' is invalid.</summary>
        internal static string @net_http_headers_invalid_value => GetResourceString("net_http_headers_invalid_value", @"The format of value '{0}' is invalid.");
        /// <summary>Misused header name, '{0}'. Make sure request headers are used with HttpRequestMessage, response headers with HttpResponseMessage, and content headers with HttpContent objects.</summary>
        internal static string @net_http_headers_not_allowed_header_name => GetResourceString("net_http_headers_not_allowed_header_name", @"Misused header name, '{0}'. Make sure request headers are used with HttpRequestMessage, response headers with HttpResponseMessage, and content headers with HttpContent objects.");
        /// <summary>The specified value is not a valid 'Host' header string.</summary>
        internal static string @net_http_headers_invalid_host_header => GetResourceString("net_http_headers_invalid_host_header", @"The specified value is not a valid 'Host' header string.");
        /// <summary>The specified value is not a valid 'From' header string.</summary>
        internal static string @net_http_headers_invalid_from_header => GetResourceString("net_http_headers_invalid_from_header", @"The specified value is not a valid 'From' header string.");
        /// <summary>The specified value is not a valid quoted string.</summary>
        internal static string @net_http_headers_invalid_etag_name => GetResourceString("net_http_headers_invalid_etag_name", @"The specified value is not a valid quoted string.");
        /// <summary>Invalid range. At least one of the two parameters must not be null.</summary>
        internal static string @net_http_headers_invalid_range => GetResourceString("net_http_headers_invalid_range", @"Invalid range. At least one of the two parameters must not be null.");
        /// <summary>New-line characters in header values must be followed by a white-space character.</summary>
        internal static string @net_http_headers_no_newlines => GetResourceString("net_http_headers_no_newlines", @"New-line characters in header values must be followed by a white-space character.");
        /// <summary>Cannot write more bytes to the buffer than the configured maximum buffer size: {0}.</summary>
        internal static string @net_http_content_buffersize_exceeded => GetResourceString("net_http_content_buffersize_exceeded", @"Cannot write more bytes to the buffer than the configured maximum buffer size: {0}.");
        /// <summary>The async operation did not return a System.Threading.Tasks.Task object.</summary>
        internal static string @net_http_content_no_task_returned => GetResourceString("net_http_content_no_task_returned", @"The async operation did not return a System.Threading.Tasks.Task object.");
        /// <summary>The stream was already consumed. It cannot be read again.</summary>
        internal static string @net_http_content_stream_already_read => GetResourceString("net_http_content_stream_already_read", @"The stream was already consumed. It cannot be read again.");
        /// <summary>The stream does not support writing.</summary>
        internal static string @net_http_content_readonly_stream => GetResourceString("net_http_content_readonly_stream", @"The stream does not support writing.");
        /// <summary>The character set provided in ContentType is invalid. Cannot read content as string using an invalid character set.</summary>
        internal static string @net_http_content_invalid_charset => GetResourceString("net_http_content_invalid_charset", @"The character set provided in ContentType is invalid. Cannot read content as string using an invalid character set.");
        /// <summary>Error while copying content to a stream.</summary>
        internal static string @net_http_content_stream_copy_error => GetResourceString("net_http_content_stream_copy_error", @"Error while copying content to a stream.");
        /// <summary>The value cannot be null or empty.</summary>
        internal static string @net_http_argument_empty_string => GetResourceString("net_http_argument_empty_string", @"The value cannot be null or empty.");
        /// <summary>The request message was already sent. Cannot send the same request message multiple times.</summary>
        internal static string @net_http_client_request_already_sent => GetResourceString("net_http_client_request_already_sent", @"The request message was already sent. Cannot send the same request message multiple times.");
        /// <summary>This instance has already started one or more requests. Properties can only be modified before sending the first request.</summary>
        internal static string @net_http_operation_started => GetResourceString("net_http_operation_started", @"This instance has already started one or more requests. Properties can only be modified before sending the first request.");
        /// <summary>An error occurred while sending the request.</summary>
        internal static string @net_http_client_execution_error => GetResourceString("net_http_client_execution_error", @"An error occurred while sending the request.");
        /// <summary>The base address must be an absolute URI.</summary>
        internal static string @net_http_client_absolute_baseaddress_required => GetResourceString("net_http_client_absolute_baseaddress_required", @"The base address must be an absolute URI.");
        /// <summary>An invalid request URI was provided. The request URI must either be an absolute URI or BaseAddress must be set.</summary>
        internal static string @net_http_client_invalid_requesturi => GetResourceString("net_http_client_invalid_requesturi", @"An invalid request URI was provided. The request URI must either be an absolute URI or BaseAddress must be set.");
        /// <summary>Only 'http' and 'https' schemes are allowed.</summary>
        internal static string @net_http_client_http_baseaddress_required => GetResourceString("net_http_client_http_baseaddress_required", @"Only 'http' and 'https' schemes are allowed.");
        /// <summary>Value '{0}' is not a valid Base64 string. Error: {1}</summary>
        internal static string @net_http_parser_invalid_base64_string => GetResourceString("net_http_parser_invalid_base64_string", @"Value '{0}' is not a valid Base64 string. Error: {1}");
        /// <summary>Handler did not return a response message.</summary>
        internal static string @net_http_handler_noresponse => GetResourceString("net_http_handler_noresponse", @"Handler did not return a response message.");
        /// <summary>A request message must be provided. It cannot be null.</summary>
        internal static string @net_http_handler_norequest => GetResourceString("net_http_handler_norequest", @"A request message must be provided. It cannot be null.");
        /// <summary>Response status code does not indicate success: {0} ({1}).</summary>
        internal static string @net_http_message_not_success_statuscode => GetResourceString("net_http_message_not_success_statuscode", @"Response status code does not indicate success: {0} ({1}).");
        /// <summary>The field cannot be longer than {0} characters.</summary>
        internal static string @net_http_content_field_too_long => GetResourceString("net_http_content_field_too_long", @"The field cannot be longer than {0} characters.");
        /// <summary>Value for header '{0}' contains invalid new-line characters. Value: '{1}'.</summary>
        internal static string @net_http_log_headers_no_newlines => GetResourceString("net_http_log_headers_no_newlines", @"Value for header '{0}' contains invalid new-line characters. Value: '{1}'.");
        /// <summary>The 'q' value is invalid: '{0}'.</summary>
        internal static string @net_http_log_headers_invalid_quality => GetResourceString("net_http_log_headers_invalid_quality", @"The 'q' value is invalid: '{0}'.");
        /// <summary>Value '{0}' is not a valid email address</summary>
        internal static string @net_http_log_headers_wrong_email_format => GetResourceString("net_http_log_headers_wrong_email_format", @"Value '{0}' is not a valid email address");
        /// <summary>The inner handler has not been assigned.</summary>
        internal static string @net_http_handler_not_assigned => GetResourceString("net_http_handler_not_assigned", @"The inner handler has not been assigned.");
        /// <summary>The {0} property must be set to '{1}' to use this property.</summary>
        internal static string @net_http_invalid_enable_first => GetResourceString("net_http_invalid_enable_first", @"The {0} property must be set to '{1}' to use this property.");
        /// <summary>Buffering more than {0} bytes is not supported.</summary>
        internal static string @net_http_content_buffersize_limit => GetResourceString("net_http_content_buffersize_limit", @"Buffering more than {0} bytes is not supported.");
        /// <summary>The value '{0}' is not supported for property '{1}'.</summary>
        internal static string @net_http_value_not_supported => GetResourceString("net_http_value_not_supported", @"The value '{0}' is not supported for property '{1}'.");
        /// <summary>The read operation failed, see inner exception.</summary>
        internal static string @net_http_io_read => GetResourceString("net_http_io_read", @"The read operation failed, see inner exception.");
        /// <summary>Unable to read data from the transport connection. The connection was closed before all data could be read. Expected {0} bytes, read {1} bytes.</summary>
        internal static string @net_http_io_read_incomplete => GetResourceString("net_http_io_read_incomplete", @"Unable to read data from the transport connection. The connection was closed before all data could be read. Expected {0} bytes, read {1} bytes.");
        /// <summary>The write operation failed, see inner exception.</summary>
        internal static string @net_http_io_write => GetResourceString("net_http_io_write", @"The write operation failed, see inner exception.");
        /// <summary>'Transfer-Encoding: chunked' header can not be used when content object is not specified.</summary>
        internal static string @net_http_chunked_not_allowed_with_empty_content => GetResourceString("net_http_chunked_not_allowed_with_empty_content", @"'Transfer-Encoding: chunked' header can not be used when content object is not specified.");
        /// <summary>When using CookieUsePolicy.UseSpecifiedCookieContainer, the CookieContainer property must not be null.</summary>
        internal static string @net_http_invalid_cookiecontainer => GetResourceString("net_http_invalid_cookiecontainer", @"When using CookieUsePolicy.UseSpecifiedCookieContainer, the CookieContainer property must not be null.");
        /// <summary>When using a non-null Proxy, the WindowsProxyUsePolicy property must be set to WindowsProxyUsePolicy.UseCustomProxy.</summary>
        internal static string @net_http_invalid_proxyusepolicy => GetResourceString("net_http_invalid_proxyusepolicy", @"When using a non-null Proxy, the WindowsProxyUsePolicy property must be set to WindowsProxyUsePolicy.UseCustomProxy.");
        /// <summary>When using WindowsProxyUsePolicy.UseCustomProxy, the Proxy property must not be null.</summary>
        internal static string @net_http_invalid_proxy => GetResourceString("net_http_invalid_proxy", @"When using WindowsProxyUsePolicy.UseCustomProxy, the Proxy property must not be null.");
        /// <summary>The specified value must be greater than {0}.</summary>
        internal static string @net_http_value_must_be_greater_than => GetResourceString("net_http_value_must_be_greater_than", @"The specified value must be greater than {0}.");
        /// <summary>An invalid character was found in the mail header: '{0}'.</summary>
        internal static string @MailHeaderFieldInvalidCharacter => GetResourceString("MailHeaderFieldInvalidCharacter", @"An invalid character was found in the mail header: '{0}'.");
        /// <summary>The specified string is not in the form required for an e-mail address.</summary>
        internal static string @MailAddressInvalidFormat => GetResourceString("MailAddressInvalidFormat", @"The specified string is not in the form required for an e-mail address.");
        /// <summary>The mail header is malformed.</summary>
        internal static string @MailHeaderFieldMalformedHeader => GetResourceString("MailHeaderFieldMalformedHeader", @"The mail header is malformed.");
        /// <summary>An invalid character was found in header name.</summary>
        internal static string @InvalidHeaderName => GetResourceString("InvalidHeaderName", @"An invalid character was found in header name.");
        /// <summary>The '{0}'='{1}' part of the cookie is invalid.</summary>
        internal static string @net_cookie_attribute => GetResourceString("net_cookie_attribute", @"The '{0}'='{1}' part of the cookie is invalid.");
        /// <summary>Specified file length was too large for the file system.</summary>
        internal static string @ArgumentOutOfRange_FileLengthTooBig => GetResourceString("ArgumentOutOfRange_FileLengthTooBig", @"Specified file length was too large for the file system.");
        /// <summary>The file '{0}' already exists.</summary>
        internal static string @IO_FileExists_Name => GetResourceString("IO_FileExists_Name", @"The file '{0}' already exists.");
        /// <summary>Unable to find the specified file.</summary>
        internal static string @IO_FileNotFound => GetResourceString("IO_FileNotFound", @"Unable to find the specified file.");
        /// <summary>Could not find file '{0}'.</summary>
        internal static string @IO_FileNotFound_FileName => GetResourceString("IO_FileNotFound_FileName", @"Could not find file '{0}'.");
        /// <summary>Could not find a part of the path.</summary>
        internal static string @IO_PathNotFound_NoPathName => GetResourceString("IO_PathNotFound_NoPathName", @"Could not find a part of the path.");
        /// <summary>Could not find a part of the path '{0}'.</summary>
        internal static string @IO_PathNotFound_Path => GetResourceString("IO_PathNotFound_Path", @"Could not find a part of the path '{0}'.");
        /// <summary>The specified file name or path is too long, or a component of the specified path is too long.</summary>
        internal static string @IO_PathTooLong => GetResourceString("IO_PathTooLong", @"The specified file name or path is too long, or a component of the specified path is too long.");
        /// <summary>The process cannot access the file '{0}' because it is being used by another process.</summary>
        internal static string @IO_SharingViolation_File => GetResourceString("IO_SharingViolation_File", @"The process cannot access the file '{0}' because it is being used by another process.");
        /// <summary>The process cannot access the file because it is being used by another process.</summary>
        internal static string @IO_SharingViolation_NoFileName => GetResourceString("IO_SharingViolation_NoFileName", @"The process cannot access the file because it is being used by another process.");
        /// <summary>Access to the path is denied.</summary>
        internal static string @UnauthorizedAccess_IODenied_NoPathName => GetResourceString("UnauthorizedAccess_IODenied_NoPathName", @"Access to the path is denied.");
        /// <summary>Access to the path '{0}' is denied.</summary>
        internal static string @UnauthorizedAccess_IODenied_Path => GetResourceString("UnauthorizedAccess_IODenied_Path", @"Access to the path '{0}' is denied.");
        /// <summary>The stream does not support concurrent read operations.</summary>
        internal static string @net_http_content_no_concurrent_reads => GetResourceString("net_http_content_no_concurrent_reads", @"The stream does not support concurrent read operations.");
        /// <summary>The username for a credential object cannot be null or empty.</summary>
        internal static string @net_http_username_empty_string => GetResourceString("net_http_username_empty_string", @"The username for a credential object cannot be null or empty.");
        /// <summary>The stream does not support concurrent I/O read or write operations.</summary>
        internal static string @net_http_no_concurrent_io_allowed => GetResourceString("net_http_no_concurrent_io_allowed", @"The stream does not support concurrent I/O read or write operations.");
        /// <summary>The server returned an invalid or unrecognized response.</summary>
        internal static string @net_http_invalid_response => GetResourceString("net_http_invalid_response", @"The server returned an invalid or unrecognized response.");
        /// <summary>The response ended prematurely.</summary>
        internal static string @net_http_invalid_response_premature_eof => GetResourceString("net_http_invalid_response_premature_eof", @"The response ended prematurely.");
        /// <summary>The response ended prematurely while waiting for the next frame from the server.</summary>
        internal static string @net_http_invalid_response_missing_frame => GetResourceString("net_http_invalid_response_missing_frame", @"The response ended prematurely while waiting for the next frame from the server.");
        /// <summary>The response ended prematurely, with at least {0} additional bytes expected.</summary>
        internal static string @net_http_invalid_response_premature_eof_bytecount => GetResourceString("net_http_invalid_response_premature_eof_bytecount", @"The response ended prematurely, with at least {0} additional bytes expected.");
        /// <summary>Received chunk header length could not be parsed: '{0}'.</summary>
        internal static string @net_http_invalid_response_chunk_header_invalid => GetResourceString("net_http_invalid_response_chunk_header_invalid", @"Received chunk header length could not be parsed: '{0}'.");
        /// <summary>Received an invalid chunk extension: '{0}'.</summary>
        internal static string @net_http_invalid_response_chunk_extension_invalid => GetResourceString("net_http_invalid_response_chunk_extension_invalid", @"Received an invalid chunk extension: '{0}'.");
        /// <summary>Received an invalid chunk terminator: '{0}'.</summary>
        internal static string @net_http_invalid_response_chunk_terminator_invalid => GetResourceString("net_http_invalid_response_chunk_terminator_invalid", @"Received an invalid chunk terminator: '{0}'.");
        /// <summary>Received an invalid status line: '{0}'.</summary>
        internal static string @net_http_invalid_response_status_line => GetResourceString("net_http_invalid_response_status_line", @"Received an invalid status line: '{0}'.");
        /// <summary>Received an invalid status code: '{0}'.</summary>
        internal static string @net_http_invalid_response_status_code => GetResourceString("net_http_invalid_response_status_code", @"Received an invalid status code: '{0}'.");
        /// <summary>Received status phrase could not be decoded with iso-8859-1: '{0}'.</summary>
        internal static string @net_http_invalid_response_status_reason => GetResourceString("net_http_invalid_response_status_reason", @"Received status phrase could not be decoded with iso-8859-1: '{0}'.");
        /// <summary>Received an invalid folded header.</summary>
        internal static string @net_http_invalid_response_header_folder => GetResourceString("net_http_invalid_response_header_folder", @"Received an invalid folded header.");
        /// <summary>Received an invalid header line: '{0}'.</summary>
        internal static string @net_http_invalid_response_header_line => GetResourceString("net_http_invalid_response_header_line", @"Received an invalid header line: '{0}'.");
        /// <summary>Received an invalid header name: '{0}'.</summary>
        internal static string @net_http_invalid_response_header_name => GetResourceString("net_http_invalid_response_header_name", @"Received an invalid header name: '{0}'.");
        /// <summary>The request was aborted.</summary>
        internal static string @net_http_request_aborted => GetResourceString("net_http_request_aborted", @"The request was aborted.");
        /// <summary>Received an HTTP/2 pseudo-header as a trailing header.</summary>
        internal static string @net_http_invalid_response_pseudo_header_in_trailer => GetResourceString("net_http_invalid_response_pseudo_header_in_trailer", @"Received an HTTP/2 pseudo-header as a trailing header.");
        /// <summary>The handler was disposed of while active operations were in progress.</summary>
        internal static string @net_http_unix_handler_disposed => GetResourceString("net_http_unix_handler_disposed", @"The handler was disposed of while active operations were in progress.");
        /// <summary>The buffer was not long enough.</summary>
        internal static string @net_http_buffer_insufficient_length => GetResourceString("net_http_buffer_insufficient_length", @"The buffer was not long enough.");
        /// <summary>The HTTP response headers length exceeded the set limit of {0} bytes.</summary>
        internal static string @net_http_response_headers_exceeded_length => GetResourceString("net_http_response_headers_exceeded_length", @"The HTTP response headers length exceeded the set limit of {0} bytes.");
        /// <summary>Non-negative number required.</summary>
        internal static string @ArgumentOutOfRange_NeedNonNegativeNum => GetResourceString("ArgumentOutOfRange_NeedNonNegativeNum", @"Non-negative number required.");
        /// <summary>Positive number required.</summary>
        internal static string @ArgumentOutOfRange_NeedPosNum => GetResourceString("ArgumentOutOfRange_NeedPosNum", @"Positive number required.");
        /// <summary>Stream does not support reading.</summary>
        internal static string @NotSupported_UnreadableStream => GetResourceString("NotSupported_UnreadableStream", @"Stream does not support reading.");
        /// <summary>Stream does not support writing.</summary>
        internal static string @NotSupported_UnwritableStream => GetResourceString("NotSupported_UnwritableStream", @"Stream does not support writing.");
        /// <summary>Cannot access a closed stream.</summary>
        internal static string @ObjectDisposed_StreamClosed => GetResourceString("ObjectDisposed_StreamClosed", @"Cannot access a closed stream.");
        /// <summary>Using this feature requires Windows 10 Version 1607.</summary>
        internal static string @net_http_feature_requires_Windows10Version1607 => GetResourceString("net_http_feature_requires_Windows10Version1607", @"Using this feature requires Windows 10 Version 1607.");
        /// <summary>Client certificate was not found in the personal (\"MY\") certificate store. In UWP, client certificates are only supported if they have been added to that certificate store.</summary>
        internal static string @net_http_feature_UWPClientCertSupportRequiresCertInPersonalCertificateStore => GetResourceString("net_http_feature_UWPClientCertSupportRequiresCertInPersonalCertificateStore", @"Client certificate was not found in the personal (\""MY\"") certificate store. In UWP, client certificates are only supported if they have been added to that certificate store.");
        /// <summary>Only the 'http' scheme is allowed for proxies.</summary>
        internal static string @net_http_invalid_proxy_scheme => GetResourceString("net_http_invalid_proxy_scheme", @"Only the 'http' scheme is allowed for proxies.");
        /// <summary>Request headers must contain only ASCII characters.</summary>
        internal static string @net_http_request_invalid_char_encoding => GetResourceString("net_http_request_invalid_char_encoding", @"Request headers must contain only ASCII characters.");
        /// <summary>The SSL connection could not be established, see inner exception.</summary>
        internal static string @net_http_ssl_connection_failed => GetResourceString("net_http_ssl_connection_failed", @"The SSL connection could not be established, see inner exception.");
        /// <summary>HTTP 1.0 does not support chunking.</summary>
        internal static string @net_http_unsupported_chunking => GetResourceString("net_http_unsupported_chunking", @"HTTP 1.0 does not support chunking.");
        /// <summary>Request HttpVersion 0.X is not supported.  Use 1.0 or above.</summary>
        internal static string @net_http_unsupported_version => GetResourceString("net_http_unsupported_version", @"Request HttpVersion 0.X is not supported.  Use 1.0 or above.");
        /// <summary>An attempt was made to move the position before the beginning of the stream.</summary>
        internal static string @IO_SeekBeforeBegin => GetResourceString("IO_SeekBeforeBegin", @"An attempt was made to move the position before the beginning of the stream.");
        /// <summary>The application protocol list is invalid.</summary>
        internal static string @net_ssl_app_protocols_invalid => GetResourceString("net_ssl_app_protocols_invalid", @"The application protocol list is invalid.");
        /// <summary>HTTP/2 requires TLS 1.2 or newer, but '{0}' was negotiated.</summary>
        internal static string @net_ssl_http2_requires_tls12 => GetResourceString("net_ssl_http2_requires_tls12", @"HTTP/2 requires TLS 1.2 or newer, but '{0}' was negotiated.");
        /// <summary>The path '{0}' is too long, or a component of the specified path is too long.</summary>
        internal static string @IO_PathTooLong_Path => GetResourceString("IO_PathTooLong_Path", @"The path '{0}' is too long, or a component of the specified path is too long.");
        /// <summary>CONNECT request must contain Host header.</summary>
        internal static string @net_http_request_no_host => GetResourceString("net_http_request_no_host", @"CONNECT request must contain Host header.");
        /// <summary>Error {0} calling {1}, '{2}'.</summary>
        internal static string @net_http_winhttp_error => GetResourceString("net_http_winhttp_error", @"Error {0} calling {1}, '{2}'.");
        /// <summary>The HTTP/2 server sent invalid data on the connection. HTTP/2 error code '{0}' (0x{1}).</summary>
        internal static string @net_http_http2_connection_error => GetResourceString("net_http_http2_connection_error", @"The HTTP/2 server sent invalid data on the connection. HTTP/2 error code '{0}' (0x{1}).");
        /// <summary>The HTTP/2 server reset the stream. HTTP/2 error code '{0}' (0x{1}).</summary>
        internal static string @net_http_http2_stream_error => GetResourceString("net_http_http2_stream_error", @"The HTTP/2 server reset the stream. HTTP/2 error code '{0}' (0x{1}).");
        /// <summary>This method is not implemented by this class.</summary>
        internal static string @net_MethodNotImplementedException => GetResourceString("net_MethodNotImplementedException", @"This method is not implemented by this class.");
        /// <summary>{0} returned {1}.</summary>
        internal static string @event_OperationReturnedSomething => GetResourceString("event_OperationReturnedSomething", @"{0} returned {1}.");
        /// <summary>{0} failed with error {1}.</summary>
        internal static string @net_log_operation_failed_with_error => GetResourceString("net_log_operation_failed_with_error", @"{0} failed with error {1}.");
        /// <summary>This operation cannot be performed on a completed asynchronous result object.</summary>
        internal static string @net_completed_result => GetResourceString("net_completed_result", @"This operation cannot be performed on a completed asynchronous result object.");
        /// <summary>The specified value is not valid in the '{0}' enumeration.</summary>
        internal static string @net_invalid_enum => GetResourceString("net_invalid_enum", @"The specified value is not valid in the '{0}' enumeration.");
        /// <summary>Protocol error: A received message contains a valid signature but it was not encrypted as required by the effective Protection Level.</summary>
        internal static string @net_auth_message_not_encrypted => GetResourceString("net_auth_message_not_encrypted", @"Protocol error: A received message contains a valid signature but it was not encrypted as required by the effective Protection Level.");
        /// <summary>The requested security package is not supported.</summary>
        internal static string @net_securitypackagesupport => GetResourceString("net_securitypackagesupport", @"The requested security package is not supported.");
        /// <summary>'{0}' is not a supported handle type.</summary>
        internal static string @SSPIInvalidHandleType => GetResourceString("SSPIInvalidHandleType", @"'{0}' is not a supported handle type.");
        /// <summary>Authentication failed because the connection could not be reused.</summary>
        internal static string @net_http_authconnectionfailure => GetResourceString("net_http_authconnectionfailure", @"Authentication failed because the connection could not be reused.");
        /// <summary>Server implementation is not supported</summary>
        internal static string @net_nego_server_not_supported => GetResourceString("net_nego_server_not_supported", @"Server implementation is not supported");
        /// <summary>Requested protection level is not supported with the GSSAPI implementation currently installed.</summary>
        internal static string @net_nego_protection_level_not_supported => GetResourceString("net_nego_protection_level_not_supported", @"Requested protection level is not supported with the GSSAPI implementation currently installed.");
        /// <summary>Insufficient buffer space. Required: {0} Actual: {1}.</summary>
        internal static string @net_context_buffer_too_small => GetResourceString("net_context_buffer_too_small", @"Insufficient buffer space. Required: {0} Actual: {1}.");
        /// <summary>GSSAPI operation failed with error - {0} ({1}).</summary>
        internal static string @net_gssapi_operation_failed_detailed => GetResourceString("net_gssapi_operation_failed_detailed", @"GSSAPI operation failed with error - {0} ({1}).");
        /// <summary>GSSAPI operation failed with status: {0} (Minor status: {1}).</summary>
        internal static string @net_gssapi_operation_failed => GetResourceString("net_gssapi_operation_failed", @"GSSAPI operation failed with status: {0} (Minor status: {1}).");
        /// <summary>GSSAPI operation failed with error - {0}.</summary>
        internal static string @net_gssapi_operation_failed_detailed_majoronly => GetResourceString("net_gssapi_operation_failed_detailed_majoronly", @"GSSAPI operation failed with error - {0}.");
        /// <summary>GSSAPI operation failed with status: {0}.</summary>
        internal static string @net_gssapi_operation_failed_majoronly => GetResourceString("net_gssapi_operation_failed_majoronly", @"GSSAPI operation failed with status: {0}.");
        /// <summary>NTLM authentication requires the GSSAPI plugin 'gss-ntlmssp'.</summary>
        internal static string @net_gssapi_ntlm_missing_plugin => GetResourceString("net_gssapi_ntlm_missing_plugin", @"NTLM authentication requires the GSSAPI plugin 'gss-ntlmssp'.");
        /// <summary>NTLM authentication is not possible with default credentials on this platform.</summary>
        internal static string @net_ntlm_not_possible_default_cred => GetResourceString("net_ntlm_not_possible_default_cred", @"NTLM authentication is not possible with default credentials on this platform.");
        /// <summary>Target name should be non empty if default credentials are passed.</summary>
        internal static string @net_nego_not_supported_empty_target_with_defaultcreds => GetResourceString("net_nego_not_supported_empty_target_with_defaultcreds", @"Target name should be non empty if default credentials are passed.");
        /// <summary>Huffman-coded literal string failed to decode.</summary>
        internal static string @net_http_hpack_huffman_decode_failed => GetResourceString("net_http_hpack_huffman_decode_failed", @"Huffman-coded literal string failed to decode.");
        /// <summary>Incomplete header block received.</summary>
        internal static string @net_http_hpack_incomplete_header_block => GetResourceString("net_http_hpack_incomplete_header_block", @"Incomplete header block received.");
        /// <summary>Dynamic table size update received after beginning of header block.</summary>
        internal static string @net_http_hpack_late_dynamic_table_size_update => GetResourceString("net_http_hpack_late_dynamic_table_size_update", @"Dynamic table size update received after beginning of header block.");
        /// <summary>HPACK integer exceeds limits or has an overlong encoding.</summary>
        internal static string @net_http_hpack_bad_integer => GetResourceString("net_http_hpack_bad_integer", @"HPACK integer exceeds limits or has an overlong encoding.");
        /// <summary>The object was disposed while operations were in progress.</summary>
        internal static string @net_http_disposed_while_in_use => GetResourceString("net_http_disposed_while_in_use", @"The object was disposed while operations were in progress.");
        /// <summary>Dynamic table size update to {0} bytes exceeds limit of {1} bytes.</summary>
        internal static string @net_http_hpack_large_table_size_update => GetResourceString("net_http_hpack_large_table_size_update", @"Dynamic table size update to {0} bytes exceeds limit of {1} bytes.");
        /// <summary>The server shut down the connection.</summary>
        internal static string @net_http_server_shutdown => GetResourceString("net_http_server_shutdown", @"The server shut down the connection.");
        /// <summary>Invalid header index: {0} is outside of static table and no dynamic table entry found.</summary>
        internal static string @net_http_hpack_invalid_index => GetResourceString("net_http_hpack_invalid_index", @"Invalid header index: {0} is outside of static table and no dynamic table entry found.");
        /// <summary>End of headers reached with incomplete token.</summary>
        internal static string @net_http_hpack_unexpected_end => GetResourceString("net_http_hpack_unexpected_end", @"End of headers reached with incomplete token.");
        /// <summary>Failed to HPACK encode the headers.</summary>
        internal static string @net_http_hpack_encode_failure => GetResourceString("net_http_hpack_encode_failure", @"Failed to HPACK encode the headers.");
        /// <summary>The HTTP headers length exceeded the set limit of {0} bytes.</summary>
        internal static string @net_http_headers_exceeded_length => GetResourceString("net_http_headers_exceeded_length", @"The HTTP headers length exceeded the set limit of {0} bytes.");
        /// <summary>Received an invalid header name: '{0}'.</summary>
        internal static string @net_http_invalid_header_name => GetResourceString("net_http_invalid_header_name", @"Received an invalid header name: '{0}'.");
        /// <summary>The HTTP/3 server sent invalid data on the connection. HTTP/3 error code '{0}' (0x{1}).</summary>
        internal static string @net_http_http3_connection_error => GetResourceString("net_http_http3_connection_error", @"The HTTP/3 server sent invalid data on the connection. HTTP/3 error code '{0}' (0x{1}).");
        /// <summary>The HTTP/3 server sent invalid data on the stream. HTTP/3 error code '{0}' (0x{1}).</summary>
        internal static string @net_http_http3_stream_error => GetResourceString("net_http_http3_stream_error", @"The HTTP/3 server sent invalid data on the stream. HTTP/3 error code '{0}' (0x{1}).");
        /// <summary>The server is unable to process the request using the current HTTP version and indicates the request should be retried on an older HTTP version.</summary>
        internal static string @net_http_retry_on_older_version => GetResourceString("net_http_retry_on_older_version", @"The server is unable to process the request using the current HTTP version and indicates the request should be retried on an older HTTP version.");
        /// <summary>Unable to write content to request stream; content would exceed Content-Length.</summary>
        internal static string @net_http_content_write_larger_than_content_length => GetResourceString("net_http_content_write_larger_than_content_length", @"Unable to write content to request stream; content would exceed Content-Length.");
        /// <summary>The HTTP/3 server attempted to reference a dynamic table index that does not exist.</summary>
        internal static string @net_http_qpack_no_dynamic_table => GetResourceString("net_http_qpack_no_dynamic_table", @"The HTTP/3 server attempted to reference a dynamic table index that does not exist.");
        /// <summary>The request was canceled due to the configured HttpClient.Timeout of {0} seconds elapsing.</summary>
        internal static string @net_http_request_timedout => GetResourceString("net_http_request_timedout", @"The request was canceled due to the configured HttpClient.Timeout of {0} seconds elapsing.");
        /// <summary>Connection aborted by peer ({0}).</summary>
        internal static string @net_quic_connectionaborted => GetResourceString("net_quic_connectionaborted", @"Connection aborted by peer ({0}).");
        /// <summary>QUIC is not supported on this platform. See http://aka.ms/dotnetquic</summary>
        internal static string @net_quic_notsupported => GetResourceString("net_quic_notsupported", @"QUIC is not supported on this platform. See http://aka.ms/dotnetquic");
        /// <summary>Operation aborted.</summary>
        internal static string @net_quic_operationaborted => GetResourceString("net_quic_operationaborted", @"Operation aborted.");
        /// <summary>Stream aborted by peer ({0}).</summary>
        internal static string @net_quic_streamaborted => GetResourceString("net_quic_streamaborted", @"Stream aborted by peer ({0}).");

    }
}
