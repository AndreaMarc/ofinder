ARG IMAGE
FROM $IMAGE


ARG BUILD_ENV
COPY . .
WORKDIR /WEB

RUN echo "BUILD ENVIRONMENT -> $BUILD_ENV "
RUN npm install
RUN if [ "$BUILD_ENV" = "dev" ] ; then ember build --environment=development ; fi
RUN if [ "$BUILD_ENV" = "test" ] ; then ember build --environment=test-publish ; fi
RUN if [ "$BUILD_ENV" = "prod" ] ; then ember build --environment=production ; fi

#EXPOSE 4200 7020 7357
EXPOSE 4200

WORKDIR /WEB/dist
CMD ["ember", "server"]
