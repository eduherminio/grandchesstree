import React from "react";
import Leaderboard from "./Leaderboard";
import RealtimeStats from "./RealtimeStats";
import { useParams } from "react-router-dom";
import NavBar from "./NavBar";
import AboutCard from "./AboutCard";

const Perft: React.FC = () => {
  const { id } = useParams<{ id: string }>(); // Get id as string from URL

  // Convert the id to an integer
  const idInt = id ? parseInt(id, 10) : NaN;
  if (isNaN(idInt)) {
    return (
      <>
        <div>
          <NavBar />
          <div className="flex flex-col m-4 space-y-4 mt-20">
            <AboutCard />
          </div>
        </div>
      </>
    );
  }
  return (
    <>
      <div>
        <NavBar />
        <div className="flex flex-col m-4 space-y-4 mt-20">
          <AboutCard />

          {/* Conditionally render content based on whether idInt is valid */}
          <div className="flex flex-col md:flex-row space-x-4 space-y-4">
            <div className="flex flex-col space-y-4">
              {/* Render components when idInt is valid */}
              <RealtimeStats id={idInt} />
              <div className="space-y-4 p-4 bg-gray-100 rounded-lg text-gray-700 flex flex-col justify-between items-center space-x-4">
                <span className="text-md font-bold">Want to get involved?</span>
                <span className="text-sm font-semibold">
                  If you're interested in volunteering computing resources or
                  collaborating on the project, please{" "}
                  <a
                    className="font-medium text-blue-600 hover:underline"
                    href="https://discord.gg/cTu3aeCZVe"
                    target="_blank"
                    rel="noopener noreferrer"
                  >
                    join the Discord server!
                  </a>
                </span>
              </div>
            </div>
            <div className="flex-1">
              <Leaderboard id={idInt} />
            </div>
          </div>
        </div>
      </div>
    </>
  );
};

export default Perft;
