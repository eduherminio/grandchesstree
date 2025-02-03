import React, { useEffect, useState } from 'react';

// Type definition for the leaderboard response
interface PerftStatsResponse {
  nps: number;
  tpm: number;
  completed_tasks: number;
  percent_completed_tasks: number;
}

const RealtimeStats: React.FC = () => {
  const [leaderboardData, setLeaderboardData] = useState<PerftStatsResponse>();
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    // Fetch the leaderboard data from the API
    const fetchLeaderboard = async () => {
      try {
        const resp = await fetch("http://localhost:5032/api/v1/perft/9/stats");
        if (!resp.ok) {
          throw new Error('Failed to fetch data');
        }
        const data: PerftStatsResponse = await resp.json(); // Explicitly typing the response
        setLeaderboardData(data); // Store the data in state
      } catch (err: any) {
        setError(err.message); // Store error message if there's an issue with the fetch
      } finally {
        setLoading(false); // Set loading to false once data is fetched
      }
    };

    fetchLeaderboard();
  }, []); // Empty dependency array ensures this effect runs only once when the component mounts

// Format large numbers (e.g., 1000 -> 1k, 1000000 -> 1m)
const formatBigNumber = (num: number): string => {
  if (num >= 1e12) return (num / 1e12).toFixed(1) + 't'; // Trillion
  if (num >= 1e9) return (num / 1e9).toFixed(1) + 'b';  // Billion
  if (num >= 1e6) return (num / 1e6).toFixed(1) + 'm';  // Million
  if (num >= 1e3) return (num / 1e3).toFixed(1) + 'k';  // Thousand
  return num.toString(); // Return as is if it's less than 1000
};



  if (loading) {
    return <p>Loading...</p>;
  }

  if (error) {
    return <p>Error: {error}</p>;
  }

  return (
    <>
    <div className='m-4 flex flex-col space-y-2'>
      <p>nodes per second: {leaderboardData && formatBigNumber(leaderboardData?.nps)}</p>
      <p>tasks per minute: {leaderboardData && formatBigNumber(leaderboardData?.tpm)}</p>
      <p>completed tasks: {leaderboardData && leaderboardData?.completed_tasks}/101240</p>

      <div
  className="bg-blue-600 text-xs font-medium text-blue-100 text-center p-0.5 leading-none rounded-full"
  style={{ width: `${leaderboardData?.percent_completed_tasks}%` }}
>
  {leaderboardData && Math.round(leaderboardData?.percent_completed_tasks)}%
</div>

    </div>
    </>
  );
};

export default RealtimeStats;
