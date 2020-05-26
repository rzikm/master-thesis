/*
 * Native part of the System.Net.Quic library
 */

#ifndef SYSTEM_NET_QUIC_NATIVE_LIBRARY_H
#define SYSTEM_NET_QUIC_NATIVE_LIBRARY_H

#include <stdint.h>
#include <stddef.h>
#include <openssl/ssl.h>

void* QuicNative_TLS_method();
int QuicNative_CRYPTO_get_ex_new_index(int classIndex, long argl, void* argp, void* new_func, void* dup_func, void* free_func);
void* QuicNative_SSL_CTX_new(void* method);
void QuicNative_SSL_CTX_free(void* ctx);
void* QuicNative_SSL_new(void* ctx);
void QuicNative_SSL_free(void* ssl);
int QuicNative_SSL_use_certificate_file(void* ssl, const char* file, int fileType);
int QuicNative_SSL_use_PrivateKey_file(void* ssl, const char* file, int fileType);
int QuicNative_SSL_use_cert_and_key(void* ssl, void* x509, void* privateKey, void* caChain, int override);
int QuicNative_SSL_use_certificate(void* ssl, void* x509);
int QuicNative_SSL_set_quic_method(void* ssl, void* method);
void QuicNative_SSL_set_accept_state(void* ssl);
void QuicNative_SSL_set_connect_state(void* ssl);
int QuicNative_SSL_do_handshake(void* ssl);
int QuicNative_SSL_ctrl(void* ssl, int cmd, long larg, void* parg);
int QuicNative_SSL_callback_ctrl(void* ssl, int cmd, void* fp);
int QuicNative_SSL_get_error(void* ssl, int code);
int QuicNative_SSL_provide_quic_data(void* ssl, int level, const uint8_t* data, size_t len);
int QuicNative_SSL_set_ex_data(void* ssl, int idx, void* data);
void* QuicNative_SSL_get_ex_data(void* ssl, int idx);
int QuicNative_SSL_set_quic_transport_params(void* ssl, const uint8_t* param, size_t len);
void QuicNative_SSL_get_peer_quic_transport_params(void* ssl, const uint8_t** param, size_t *len);
int QuicNative_SSL_quic_write_level(void* ssl);
int QuicNative_SSL_is_init_finished(void* ssl);
const void* QuicNative_SSL_get_current_cipher(void* ssl);
int16_t QuicNative_SSL_CIPHER_get_protocol_id(void* cipher);
int QuicNative_SSL_set_ciphersuites(void* ssl, const char* list);
int QuicNative_SSL_set_cipher_list(void* ssl, const char* list);
const void* QuicNative_SSL_get_cipher_list(void* ssl, int priority);
int QuicNative_SSL_set_alpn_protos(void* ssl, const char* protos, int len);
void QuicNative_SSL_get0_alpn_selected(void* ssl, const unsigned char** data, int* len);
void* QuicNative_BIO_new_mem_buf(const uint8_t* buf, int len);
void QuicNative_BIO_free(void* bio);
void* QuicNative_d2i_PKCS12(void* out, const uint8_t** buf, int len);
int QuicNative_PKCS12_parse(void* pkcs, const char* pass, EVP_PKEY ** outKey, X509** outCert, STACK_OF(X509)** outCa);
void QuicNative_PKCS12_free(void* pkcs);
void QuicNative_X509_free(void* x509);
void QuicNative_EVP_PKEY_free(void* evp);
const char* QuicNative_SSL_get_servername(void* ssl, int type);

#endif //SYSTEM_NET_QUIC_NATIVE_LIBRARY_H
