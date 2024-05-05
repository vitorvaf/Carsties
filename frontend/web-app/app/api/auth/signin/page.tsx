import EmptyFilter from '@/app/components/EmptyFilter';
import React from 'react';

export default function Page({searchParams} : {searchParams: { callbackUrl: string }}){
    return (
        <EmptyFilter 
        title='You need to be logged in to do that'
        subTitle='Please click the button below to login or create an account'
        showLogin 
        callbackUrl={searchParams.callbackUrl}
        />
    )
}