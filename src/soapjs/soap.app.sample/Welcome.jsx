import React from 'react';
import WelcomeImage from 'url:./assets/images/hello_world.png';
import {translate, keys, useQuery, getHeader, headerKeys } from '@soap/modules';


function Welcome() {

    const c100v1Ping = {
        $type: 'Soap.Api.Sample.Messages.Commands.C100v1Ping, Soap.Api.Sample.Messages',
        pingedAt: new Date().toISOString(),
        pingedBy: "aaron",
        headers: []
    };
    
    const pong = useQuery(c100v1Ping);
    
    if (!pong) return (<h1>Loading...</h1>);
    
    return (
    <div>
        <img src={WelcomeImage} alt="Logo"/>
        <h1>{translate(keys.back)}</h1>
        <h2>{getHeader(pong, headerKeys.messageId)}</h2>
    </div>
    );
}

export default Welcome;
