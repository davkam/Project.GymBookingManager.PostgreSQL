--
-- PostgreSQL database dump
--

-- Dumped from database version 17.0
-- Dumped by pg_dump version 17.0

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET transaction_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

SET default_tablespace = '';

SET default_table_access_method = heap;

--
-- Name: activities; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.activities (
    id integer NOT NULL,
    name character varying(100) NOT NULL,
    description text,
    is_open boolean DEFAULT true,
    participant_limit integer,
    instructor_id integer,
    activity_date_from timestamp without time zone NOT NULL,
    activity_date_to timestamp without time zone NOT NULL,
    reservation_id integer
);


ALTER TABLE public.activities OWNER TO postgres;

--
-- Name: activity_participants; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.activity_participants (
    activity_id integer NOT NULL,
    customer_id integer NOT NULL
);


ALTER TABLE public.activity_participants OWNER TO postgres;

--
-- Name: reservables; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.reservables (
    id integer NOT NULL,
    name character varying(100) NOT NULL,
    description text,
    is_available boolean DEFAULT true,
    reservable_type character varying(10) NOT NULL,
    members_only boolean DEFAULT true,
    capacity integer,
    instructor_id integer,
    CONSTRAINT reservables_reservable_type_check CHECK (((reservable_type)::text = ANY ((ARRAY['equipment'::character varying, 'space'::character varying, 'ptrainer'::character varying])::text[])))
);


ALTER TABLE public.reservables OWNER TO postgres;

--
-- Name: reservation_reservables; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.reservation_reservables (
    reservation_id integer NOT NULL,
    reservable_id integer NOT NULL
);


ALTER TABLE public.reservation_reservables OWNER TO postgres;

--
-- Name: reservations; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.reservations (
    id integer NOT NULL,
    owner_id integer,
    reservation_date_from timestamp without time zone NOT NULL,
    reservation_date_to timestamp without time zone NOT NULL
);


ALTER TABLE public.reservations OWNER TO postgres;

--
-- Name: users; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.users (
    id integer NOT NULL,
    first_name character varying(100) NOT NULL,
    last_name character varying(100) NOT NULL,
    ssn character(15) NOT NULL,
    phone character varying(20),
    email character varying(255),
    login_name character varying(100) NOT NULL,
    login_password text NOT NULL,
    user_type character varying(10) NOT NULL,
    sub_start timestamp without time zone,
    sub_end timestamp without time zone,
    is_sub boolean DEFAULT false NOT NULL,
    is_guest boolean DEFAULT false NOT NULL,
    CONSTRAINT users_user_type_check CHECK (((user_type)::text = ANY ((ARRAY['admin'::character varying, 'staff'::character varying, 'customer'::character varying])::text[])))
);


ALTER TABLE public.users OWNER TO postgres;

--
-- Data for Name: activities; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.activities (id, name, description, is_open, participant_limit, instructor_id, activity_date_from, activity_date_to, reservation_id) FROM stdin;
0	Football Session	A game of football	t	22	3	2024-12-06 12:00:00	2024-12-06 14:00:00	0
1	Boxing	Boxing with PT	t	10	3	2024-11-30 10:00:00	2024-11-30 14:00:00	1
\.


--
-- Data for Name: activity_participants; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.activity_participants (activity_id, customer_id) FROM stdin;
1	6
\.


--
-- Data for Name: reservables; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.reservables (id, name, description, is_available, reservable_type, members_only, capacity, instructor_id) FROM stdin;
0	Football	One ball	t	equipment	t	\N	\N
1	Boxing Gloves	A pair of boxing gloves	t	equipment	f	\N	\N
2	Football Hall	A medium sports hall used for football	t	space	t	25	\N
3	Boxing Gym	Gym used for boxing	t	space	t	10	\N
4	Amanda Andersson	073-4587263, amanda.andersson@mail.com	t	ptrainer	t	\N	2
\.


--
-- Data for Name: reservation_reservables; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.reservation_reservables (reservation_id, reservable_id) FROM stdin;
0	2
0	0
1	3
1	1
1	4
2	4
\.


--
-- Data for Name: reservations; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.reservations (id, owner_id, reservation_date_from, reservation_date_to) FROM stdin;
0	3	2024-12-06 12:00:00	2024-12-06 14:00:00
1	3	2024-11-30 10:00:00	2024-11-30 14:00:00
2	1	2024-12-02 12:00:00	2024-12-02 16:00:00
\.


--
-- Data for Name: users; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.users (id, first_name, last_name, ssn, phone, email, login_name, login_password, user_type, sub_start, sub_end, is_sub, is_guest) FROM stdin;
2	Amanda	Andersson	791124-4536    	073-4587263	amanda.andersson@mail.com	amaand	abc123	staff	\N	\N	f	f
3	Harald	Karlsson	640215-2358    	070-5298163	harald.karlsson@mail.com	harkar	abc123	staff	\N	\N	f	f
5	Karin	Löfstedt	760514-1478    	073-5124892	karin.lofstedt@mail.com	karlof	abc123	customer	-infinity	-infinity	f	f
6	Pernilla	Granqvist	781104-1523    	073-2514752	pernilla.granqvist@mail.com	pergra	abc123	customer	2024-11-17 00:00:00	2025-11-17 00:00:00	t	f
4	Tommy	Nilsson	851126-1678    	070-1542782	tommy.nilsson@mail.com	tomnil	abc123	customer	2024-11-17 00:00:00	2024-12-17 00:00:00	t	f
1	Peter	Parker	750512-1589    	073-5142789	peter.parker@mail.com	petpar	abc123	staff	\N	\N	f	f
\.


--
-- Name: activities activities_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.activities
    ADD CONSTRAINT activities_pkey PRIMARY KEY (id);


--
-- Name: activity_participants activity_participants_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.activity_participants
    ADD CONSTRAINT activity_participants_pkey PRIMARY KEY (activity_id, customer_id);


--
-- Name: reservables reservables_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.reservables
    ADD CONSTRAINT reservables_pkey PRIMARY KEY (id);


--
-- Name: reservation_reservables reservation_reservables_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.reservation_reservables
    ADD CONSTRAINT reservation_reservables_pkey PRIMARY KEY (reservation_id, reservable_id);


--
-- Name: reservations reservations_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.reservations
    ADD CONSTRAINT reservations_pkey PRIMARY KEY (id);


--
-- Name: users users_email_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.users
    ADD CONSTRAINT users_email_key UNIQUE (email);


--
-- Name: users users_login_name_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.users
    ADD CONSTRAINT users_login_name_key UNIQUE (login_name);


--
-- Name: users users_phone_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.users
    ADD CONSTRAINT users_phone_key UNIQUE (phone);


--
-- Name: users users_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.users
    ADD CONSTRAINT users_pkey PRIMARY KEY (id);


--
-- Name: users users_ssn_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.users
    ADD CONSTRAINT users_ssn_key UNIQUE (ssn);


--
-- Name: activities activities_instructor_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.activities
    ADD CONSTRAINT activities_instructor_id_fkey FOREIGN KEY (instructor_id) REFERENCES public.users(id) ON DELETE CASCADE;


--
-- Name: activities activities_reservation_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.activities
    ADD CONSTRAINT activities_reservation_id_fkey FOREIGN KEY (reservation_id) REFERENCES public.reservations(id) ON DELETE CASCADE;


--
-- Name: activity_participants activity_participants_activity_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.activity_participants
    ADD CONSTRAINT activity_participants_activity_id_fkey FOREIGN KEY (activity_id) REFERENCES public.activities(id) ON DELETE CASCADE;


--
-- Name: activity_participants activity_participants_customer_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.activity_participants
    ADD CONSTRAINT activity_participants_customer_id_fkey FOREIGN KEY (customer_id) REFERENCES public.users(id) ON DELETE CASCADE;


--
-- Name: reservables reservables_instructor_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.reservables
    ADD CONSTRAINT reservables_instructor_id_fkey FOREIGN KEY (instructor_id) REFERENCES public.users(id) ON DELETE CASCADE;


--
-- Name: reservation_reservables reservation_reservables_reservable_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.reservation_reservables
    ADD CONSTRAINT reservation_reservables_reservable_id_fkey FOREIGN KEY (reservable_id) REFERENCES public.reservables(id) ON DELETE CASCADE;


--
-- Name: reservation_reservables reservation_reservables_reservation_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.reservation_reservables
    ADD CONSTRAINT reservation_reservables_reservation_id_fkey FOREIGN KEY (reservation_id) REFERENCES public.reservations(id) ON DELETE CASCADE;


--
-- Name: reservations reservations_owner_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.reservations
    ADD CONSTRAINT reservations_owner_id_fkey FOREIGN KEY (owner_id) REFERENCES public.users(id) ON DELETE CASCADE;


--
-- PostgreSQL database dump complete
--

