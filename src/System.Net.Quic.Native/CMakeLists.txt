cmake_minimum_required(VERSION 3.12)
project(System.Net.Quic.Native C)

set(CMAKE_C_STANDARD 99)

set(OPENSSL_SOURCE_DIR "${CMAKE_CURRENT_LIST_DIR}/../../extern/akamai-openssl-quic" CACHE STRING "Path to OpenSSL source files")
if ("${CMAKE_HOST_SYSTEM_NAME}" STREQUAL "Windows")
    if (CMAKE_CL_64)
        set(ARCH_DIR "win32")
    else()
        set(ARCH_DIR "win64")
    endif()
    #TODO check these
    set(OPENSSL_STATIC_LIBS
            "${CMAKE_CURRENT_BINARY_DIR}/openssl/lib/ssl.lib"
            "${CMAKE_CURRENT_BINARY_DIR}/openssl/lib/crypto.lib")

    set(OPENSSL_CONFIG_TARGET VC-WIN64A)
    set(OPENSSL_BUILD_TOOL nmake)
else()
    set(PLATFORM "linux86_64")
    set(OPENSSL_STATIC_LIBS
            "${CMAKE_CURRENT_BINARY_DIR}/openssl/lib/libssl.a"
            "${CMAKE_CURRENT_BINARY_DIR}/openssl/lib/libcrypto.a")
    set(OPENSSL_CONFIG_TARGET linux-x86_64)
    set(OPENSSL_BUILD_TOOL make)
endif()

set(ARCH "x64" CACHE STRING "CPU architecture to build for")

set(CMAKE_LIBRARY_OUTPUT_DIRECTORY ${CMAKE_LIBRARY_OUTPUT_DIRECTORY}/${PLATFORM})


# Configure and build OpenSSL.
add_custom_command(
        WORKING_DIRECTORY ${OPENSSL_SOURCE_DIR}
        OUTPUT ${CMAKE_CURRENT_BINARY_DIR}/openssl/include
        OUTPUT ${OPENSSL_STATIC_LIBS}
        COMMAND perl ./Configure ${OPENSSL_CONFIG_TARGET} enable-tls1_3 --prefix=${CMAKE_CURRENT_BINARY_DIR}/openssl
        COMMAND ${OPENSSL_BUILD_TOOL} install_sw)

add_custom_target(OpenSSL
        DEPENDS ${OPENSSL_SOURCE_DIR}/include
        DEPENDS ${OPENSSL_STATIC_LIBS})

include_directories(${OPENSSL_SOURCE_DIR}/include)

add_library(System.Net.Quic.Native SHARED library.c library.h)
add_dependencies(System.Net.Quic.Native OpenSSL)
target_link_libraries(System.Net.Quic.Native
        ${OPENSSL_STATIC_LIBS})