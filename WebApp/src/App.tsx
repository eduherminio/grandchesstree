import React from 'react';
import Leaderboard from './Leaderboard';
import RealtimeStats from './RealtimeStats';
import GithubSvg from './assets/github-mark.svg'
import PerftResults from './PerftResults';
const App: React.FC = () => {
 return (
 <>
 
<div className=''>

<nav className="bg-white fixed w-full z-20 top-0 start-0 border-b border-gray-200">
  <div className="max-w-screen-xl flex flex-wrap items-center justify-between mx-auto p-4">
  <a href="https://grandchesstree.com/" className="flex items-center space-x-3 rtl:space-x-reverse">
      <span className="self-center text-2xl font-semibold whitespace-nowrap">The Grand Chess Tree</span>
  </a>
  <div className="flex md:order-2 space-x-3 md:space-x-0 rtl:space-x-reverse">
  <a type="button" href='https://github.com/Timmoth/grandchesstree' target='#' className="font-medium rounded-lg text-sm px-4 py-2 text-center flex items-center">
    <img src={GithubSvg} width={24} className='mx-2'/>
    Github
</a>
  </div>
  </div>
</nav>


 <div className='flex flex-col m-4 space-y-4 mt-20'>

 <div className="space-y-4 p-4 bg-gray-100 rounded-lg text-gray-700 flex flex-col justify-between items-center space-x-4">
    <span className="text-md font-bold">About</span>
    <span className="text-sm font-semibold">
  The Grand Chess Tree is a public, distributed effort to explore the depths of the game of chess. This project began from the enjoyment I found during the early stages of working on the <a className="font-medium text-blue-600 hover:underline" href="https://github.com/Timmoth/Sapling" target="_blank" rel="noopener noreferrer">Saplings</a> move generator.
</span>
    <span className="text-sm font-semibold"><a className="font-medium text-blue-600 hover:underline" href='https://www.chessprogramming.org/Perft_Results' target='#'>Find out more.</a></span>

    </div>
  <div className="flex flex-col md:flex-row space-x-4 space-y-4">
    <div className='flex flex-col space-y-4'>
 
    <RealtimeStats />
    <div className="space-y-4 p-4 bg-gray-100 rounded-lg text-gray-700 flex flex-col justify-between items-center space-x-4">
    <span className="text-md font-bold">Want to get involved?</span>
    <span className="text-sm font-semibold">
  If you're interested in volunteering computing resources or collaborating on the project, please <a className="font-medium text-blue-600 hover:underline" href="https://discord.gg/cTu3aeCZVe" target="_blank" rel="noopener noreferrer">join the Discord server!</a>
</span>
    </div>
    </div>
  <div className="flex-1">
  <Leaderboard />
  </div>

</div>
<PerftResults/>

  </div>
  </div>

  </>);
};

export default App;
