﻿using Core2D.Shapes;

namespace Core2D
{
    /// <summary>
    /// Defines connectable contract.
    /// </summary>
    public interface IConnectable
    {
        /// <summary>
        /// Connects point.
        /// </summary>
        /// <param name="point">The source point used for target connection.</param>
        /// <param name="target">The target point to connect to source point.</param>
        /// <returns>Returns true if connected successfully; otherwise, returns false.</returns>
        bool Connect(IPointShape point, IPointShape target);

        /// <summary>
        /// Disconnects point.
        /// </summary>
        /// <param name="point">The target point to disconnect.</param>
        /// <param name="result">The point created as result of disconnection.</param>
        /// <returns>Returns true if disconnected successfully; otherwise, returns false.</returns>
        bool Disconnect(IPointShape point, out IPointShape result);

        /// <summary>
        /// Disconnects all points.
        /// </summary>
        /// <returns>Returns true if disconnected successfully; otherwise, returns false.</returns>
        bool Disconnect();
    }
}
