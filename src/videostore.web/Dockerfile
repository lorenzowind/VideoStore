FROM node:16.13.2

WORKDIR /src

COPY ./videostore.web/entrypoint.sh videostore.web/
RUN chmod +x videostore.web/entrypoint.sh

ADD . .

WORKDIR /

COPY ./videostore.web/entrypoint.sh /usr/local/bin

ENTRYPOINT ["entrypoint.sh"]

WORKDIR /src/videostore.web

RUN npm install

CMD ["npm", "run", "dev"]



