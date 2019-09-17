const axios = require('axios')

module.exports = async function (context, req) {
    context.log('JavaScript HTTP trigger function processed a request.');

    var translatedAnswers = [];
    //Get translateToLanguageCode from query
    if (!req.query.translateToLanguageCode || !req.body){
        context.res = {
            status: 400,
            body: "Please pass translateToLanguageCode as query param and QnAAnswer as request body"
        };
    }
    //Get POST json payload
    else
    {
        var translatorKey = process.env.TRANSLATOR_TEXT_SUBSCRIPTION_KEY;
        var translatorEndpoint = process.env.TRANSLATOR_TEXT_ENDPOINT;
        for await (const answer of req.body.answers) {

            const translateObject = [{Text: answer.answer}]
            const route = `${translatorEndpoint}/translate?api-version=3.0&to=${req.query.translateToLanguageCode}`;

            const response = await axios({
                method: 'post',
                url: route,
                data: translateObject,
                headers: {
                'Content-Type': `application/json`,
                'Ocp-Apim-Subscription-Key':translatorKey
                },
            });
            answer.answer = response.data[0].translations[0].text;
            translatedAnswers.push(answer);
        }
    } 
    context.res = {
        // status: 200, /* Defaults to 200 */
        body: translatedAnswers
    };
}
