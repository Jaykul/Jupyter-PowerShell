FROM jaykul/powershell:dotnet-sdk-stretch as builder

# Use array to avoid Docker prepending /bin/sh -c
COPY ./Source /root/Source
COPY ./build.ps1 /root/
RUN pwsh /root/build.ps1 -Platform Linux

FROM jupyter/base-notebook:92fe05d1e7e5 as run

LABEL maintainer="Joel Bennett <Jaykul@HuddledMasses.org>" \
    org.label-schema.schema-version="1.0" \
    org.label-schema.name="jupyter-powershell" \
    description="This Dockerfile includes jupyter and the Jupyter-PowerShell kernel."

# TODO: add LABELs:
#     readme.md="https://github.com/PowerShell/PowerShell/blob/master/docker/README.md" \
#     org.label-schema.usage="https://github.com/PowerShell/PowerShell/tree/master/docker#run-the-docker-image-you-built" \
#     org.label-schema.url="https://github.com/PowerShell/PowerShell/blob/master/docker/README.md" \
#     org.label-schema.vcs-url="https://github.com/PowerShell/PowerShell" \
#     org.label-schema.vcs-ref=${VCS_REF}
#     org.label-schema.version=${POWERSHELL_VERSION} \
#     org.label-schema.docker.cmd="docker run ${IMAGE_NAME} pwsh -c '$psversiontable'" \

USER root

RUN apt-get update \
    && DEBIAN_FRONTEND=noninteractive \
        apt-get install -y --no-install-recommends \
        libc6 libgcc1 libgssapi-krb5-2 liblttng-ust0 libstdc++6 libcurl3 libunwind8 libuuid1 zlib1g libssl1.0.0 libicu55 \
#    && localedef -i ${LANGUAGE} -c -f UTF-8 -A /usr/share/locale/locale.alias ${LANGUAGE}.UTF-8 \
    && rm -rf /var/lib/apt/lists/*

COPY --from=builder /root/Output/Release/Linux /usr/src/jupyter-powershell
COPY --from=builder /root/Output/Release/Linux/kernel.json /usr/local/share/jupyter/kernels/powershell/kernel.json

# # Make sure the contents of our repo are in ${HOME}
# COPY . ${HOME}/
# RUN conda install -y -c damianavila82 rise
# RUN chown -R ${NB_UID} ${HOME} \
#     && chmod +x /usr/src/jupyter-powershell/PowerShell-Kernel \
#     && sed -i -e "s.PowerShell-Kernel./usr/src/jupyter-powershell/PowerShell-Kernel." /usr/local/share/jupyter/kernels/powershell/kernel.json
# USER ${NB_USER}