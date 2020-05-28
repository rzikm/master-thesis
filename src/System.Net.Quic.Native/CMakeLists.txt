cmake_minimum_required(VERSION 3.12)
project(System.Net.Quic.Native C)

set(CMAKE_C_STANDARD 99)

set(OPENSSL_ARTIFACT_DIR ${CMAKE_CURRENT_BINARY_DIR}/openssl-out)

set(OPENSSL_SOURCE_DIR "${CMAKE_CURRENT_LIST_DIR}/../../extern/akamai-openssl-quic" CACHE STRING "Path to OpenSSL source files")
if ("${CMAKE_HOST_SYSTEM_NAME}" STREQUAL "Windows")
    if (CMAKE_CL_64)
        set(PLATFORM "win64")
        set(OPENSSL_CONFIG_TARGET "VC-WIN64A")

    else()
        set(PLATFORM "win32")
        set(OPENSSL_CONFIG_TARGET "VC-WIN32")
    endif()

    set(OPENSSL_BUILD_TOOL nmake)
    set(OPENSSL_STATIC_LIBS
        "${OPENSSL_ARTIFACT_DIR}/lib/libssl.lib"
        "${OPENSSL_ARTIFACT_DIR}/lib/libcrypto.lib")

    # static link of openssl requires crypt32.lib, unless "no-stdio" is passed
    set(EXTRA_LIBS "crypt32.lib")
else()
    set(PLATFORM "linux86_64")
    set(OPENSSL_STATIC_LIBS
            "${OPENSSL_ARTIFACT_DIR}/lib/libssl.a"
            "${OPENSSL_ARTIFACT_DIR}/lib/libcrypto.a")
    set(OPENSSL_CONFIG_TARGET linux-x86_64)
    set(OPENSSL_BUILD_TOOL make)
endif()

# output based on the platform, but without Debug/Release configuration
set(CMAKE_LIBRARY_OUTPUT_DIRECTORY ${CMAKE_LIBRARY_OUTPUT_DIRECTORY}/${PLATFORM})
set(CMAKE_RUNTIME_OUTPUT_DIRECTORY ${CMAKE_RUNTIME_OUTPUT_DIRECTORY}/${PLATFORM})

if ("${CMAKE_HOST_SYSTEM_NAME}" STREQUAL "Windows")
    # force CMAKE not to use Debug/Release folders
        set(CMAKE_LIBRARY_OUTPUT_DIRECTORY_DEBUG ${CMAKE_LIBRARY_OUTPUT_DIRECTORY})
        set(CMAKE_RUNTIME_OUTPUT_DIRECTORY_DEBUG ${CMAKE_RUNTIME_OUTPUT_DIRECTORY})
        set(CMAKE_LIBRARY_OUTPUT_DIRECTORY_RELEASE ${CMAKE_LIBRARY_OUTPUT_DIRECTORY})
        set(CMAKE_RUNTIME_OUTPUT_DIRECTORY_RELEASE ${CMAKE_RUNTIME_OUTPUT_DIRECTORY})
endif()


set(OPENSSL_CONFIG_OPTIONS
    # target must be first
    ${OPENSSL_CONFIG_TARGET}
    # we are interested only in static libs
    no-unit-test no-shared 
    # we still need file IO
    # no-stdio
    # we communicate only through memory, no other IO needed
    no-sock no-dgram 
    # allow only TLS 1.3
    no-ssl no-dtls no-tls1 no-tls1_1 no-tls1_2
    --prefix=${OPENSSL_ARTIFACT_DIR})

# prepare subdir for out of source build of OpenSSL
file(MAKE_DIRECTORY ${CMAKE_CURRENT_BINARY_DIR}/openssl)

# Configure and build OpenSSL.
add_custom_command(
    WORKING_DIRECTORY ${CMAKE_CURRENT_BINARY_DIR}/openssl
    OUTPUT ${OPENSSL_STATIC_LIBS}
    OUTPUT ${OPENSSL_ARTIFACT_DIR}/include
    COMMAND perl ${OPENSSL_SOURCE_DIR}/Configure  ${OPENSSL_CONFIG_OPTIONS}
    COMMAND ${OPENSSL_BUILD_TOOL}
    COMMAND ${OPENSSL_BUILD_TOOL} install_sw)

add_custom_target(OpenSSL
    DEPENDS ${OPENSSL_ARTIFACT_DIR}/include
    DEPENDS ${OPENSSL_STATIC_LIBS})

include_directories(${OPENSSL_ARTIFACT_DIR}/include)

add_library(System.Net.Quic.Native SHARED library.c library.h)
add_dependencies(System.Net.Quic.Native OpenSSL)
target_link_libraries(System.Net.Quic.Native
    ${EXTRA_LIBS}
    ${OPENSSL_STATIC_LIBS})
