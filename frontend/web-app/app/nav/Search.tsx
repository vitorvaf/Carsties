'use client';

import { useParamsStore } from '@/hook/useParamStore';
import React, { useState } from 'react';
import { FaSearch } from 'react-icons/fa';

export default function Search() {

    const setParams = useParamsStore(state => state.setParams)
    const setSearchTerm = useParamsStore(state => state.setSearchTerm)
    const searchTerm = useParamsStore(state => state.searchTerm)

    function onChange(event: any) {
        setSearchTerm(event.target.value);
    }

    function search() {
        setParams({searchTerm: searchTerm});
    }

    return (
        <div className='flex w-[50%] items-center border-2 rounded-full py-2 shadow-sm'>
            <input
                onChange={onChange}
                onKeyDown={(e) => {
                    if(e.key === 'Enter') {
                        search();
                    }
                }}
                value={searchTerm}
                type='text'
                placeholder='Search'
                className='
                flex-grow
                pl-5
                bg-transparent
                focus:outline-none
                border-transparent
                focus:border-transparent
                focus:ring-0
                text-sm
                text-gray-600
            '
            />
            <button onClick={search}>
                <FaSearch size={34} className='bg-red-400 text-white rounded-full p-2 cursor-pointer mx-2' />
            </button>

        </div>
    );
}
