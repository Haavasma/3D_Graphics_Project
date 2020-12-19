package data

import (
	"net"
	"sync"
)

// SafeChannelAddressSlice Thread-safe channel list
type SafeChannelAddressSlice struct {
	V   map[string][]Pair
	Mux sync.Mutex
}

// SafeQueue safe queue containing addresses
type SafeQueue struct {
	Q   []net.Conn
	Mux sync.Mutex
}

// Pair pair
type Pair struct {
	Address   *net.UDPAddr
	Timestamp int64
}

// SafeChannelTCPConnSlice thread safe channel list of tcp connections
type SafeChannelTCPConnSlice struct {
	V   map[string][]net.Conn
	Mux sync.Mutex
}
