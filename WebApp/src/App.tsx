import React, { useEffect, useState } from 'react';
import Leaderboard from './Leaderboard';
import RealtimeStats from './RealtimeStats';

const App: React.FC = () => {
 return (
 <>
 <div>
 <h1 className="m-4">The Grand Chess Tree</h1>
 <RealtimeStats/>
  <Leaderboard/>
  </div>
  </>);
};

export default App;
