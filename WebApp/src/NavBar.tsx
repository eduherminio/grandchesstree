import React from "react";
import GithubSvg from "./assets/github-mark.svg";
import { Link } from "react-router-dom";

const NavBar: React.FC = () => {
  return (
    <>
      <nav className="bg-white fixed w-full z-20 top-0 start-0 border-b border-gray-200">
        <div className="max-w-screen-xl flex flex-wrap items-center justify-between mx-auto p-4">
          <a
            href="https://grandchesstree.com/"
            className="flex items-center space-x-3 rtl:space-x-reverse"
          >
            <span className="self-center text-2xl font-semibold whitespace-nowrap">
              The Grand Chess Tree
            </span>
          </a>

          <div className="flex md:order-2 space-x-3 md:space-x-0 rtl:space-x-reverse">
            <ul className="font-medium flex flex-col p-4 md:p-0 mt-4 border border-gray-100 rounded-lg bg-gray-50 md:flex-row md:space-x-8 rtl:space-x-reverse md:mt-0 md:border-0 md:bg-white dark:bg-gray-800 md:dark:bg-gray-900 dark:border-gray-700">
              <li>
                <Link
                  className="font-medium rounded-lg text-sm py-2 text-center flex items-center"
                  to="/"
                >
                  home
                </Link>
              </li>
              <li>
                <Link
                  className="font-medium rounded-lg text-sm py-2 text-center flex items-center"
                  to="/perft/11"
                >
                  perft(11)
                </Link>
              </li>
              <li>
                <Link
                  className="font-medium rounded-lg text-sm py-2 text-center flex items-center"
                  to="/perft/12"
                >
                  perft(12)
                </Link>
              </li>
              <li>
                <a
                  type="button"
                  href="https://timmoth.github.io/grandchesstree/"
                  target="#"
                  className="font-medium rounded-lg text-sm py-2 text-center flex items-center"
                >
                  docs
                </a>
              </li>
              <li>
                <a
                  type="button"
                  href="https://discord.com/invite/cTu3aeCZVe"
                  target="#"
                  className="font-medium rounded-lg text-sm py-2 text-center flex items-center"
                >
                  discord
                </a>
              </li>
              <li>
                <a
                  type="button"
                  href="https://github.com/Timmoth/grandchesstree"
                  target="#"
                  className="font-medium rounded-lg text-sm px-4 py-2 text-center flex items-center"
                >
                  <img src={GithubSvg} width={24} className="mx-2" />
                  Github
                </a>
              </li>
            </ul>
          </div>
        </div>
      </nav>
    </>
  );
};

export default NavBar;
