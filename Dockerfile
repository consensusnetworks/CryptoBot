FROM ubuntu:18.04

ARG BRANCH="master"

# Set the timezone.
ENV TZ=UTC
RUN ln -fs /usr/share/zoneinfo/UTC /etc/localtime

RUN apt-get update \
	&& apt-get -y install joe less gnupg ssh wget curl net-tools iputils-ping libncurses5-dev autoconf libncursesw5-dev git locales locales-all mtr-tiny dos2unix

##RUN apt-key adv --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys 3FA7E0328081BFF6A14DA29AA6A19B38D3D831EF
##RUN echo "deb http://download.mono-project.com/repo/ubuntu stable-xenial main" | tee /etc/apt/sources.list.d/mono-official-stable.list

RUN apt-key adv --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys 3FA7E0328081BFF6A14DA29AA6A19B38D3D831EF
RUN echo "deb http://download.mono-project.com/repo/ubuntu stable-bionic main" | tee /etc/apt/sources.list.d/mono-official-stable.list

RUN apt-get update \
    && apt-get -y install mono-devel

RUN mkdir -p /app && git clone --branch v3.5.2 https://github.com/SIPp/sipp.git /app/sipp
WORKDIR /app/sipp
RUN ./build.sh

WORKDIR /app
RUN git clone -b ${BRANCH} https://github.com/consensusnetworks/CryptoBot.git
RUN wget https://dist.nuget.org/win-x86-commandline/latest/nuget.exe
WORKDIR /app/CryptoBot
RUN mono ../nuget.exe restore TFA-Bot.sln && msbuild -p:Configuration=Release -property:GitCommit=$(git rev-parse HEAD) TFA-Bot.sln

RUN apt-get clean
RUN rm -rf /var/lib/apt/lists/*

COPY entrypoint.sh /entrypoint.sh
RUN chmod +x /entrypoint.sh && dos2unix /entrypoint.sh

ENTRYPOINT ["/entrypoint.sh"]
